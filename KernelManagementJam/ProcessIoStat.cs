using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace KernelManagementJam
{
    public struct ProcessIoStat
    {
        public int Pid { get; set; } // 1, first
        public int ParentPid { get; set; } // 1, first
        public bool IsAccessDenied { get; set; }
        public long StartAt { get; set; } // 22

        public int? Uid { get; set; }
        public string UserName { get; set; }

        public long IoTime { get; set; } // 42
        public long UserCpuUsage { get; set; } // 14
        public long KernelCpuUsage { get; set; } // 15
        public long RealTimePriority { get; set; } // 18
        public long Priority { get; set; } // 19
        public long MinorPageFaults { get; set; } // 10
        public long MajorPageFaults { get; set; } // 12
        public long NumThreads { get; set; } // 20

        // VmRSS, RssFile, RssShmem, VmSwap 
        public long RssMem { get; set; } // VmRSS in /proc/[pid]/status
        public long PeakWorkingSet { get; set; } // built-in implementation does not work on linux
        public long SharedMem { get; set; } // RssFile+RssShmem in /proc/[pid]/status
        public long SwappedMem { get; set; } // VmSwap@.../status
        public string Command { get; set; }
        public string Name { get; set; }
        
        // .../io
        public long ReadBytes { get; set; }
        public long WriteBytes { get; set; }
        public long ReadSysCalls { get; set; }
        public long WriteSysCalls { get; set; }
        public long ReadBlockBackedBytes { get; set; }
        public long WriteBlockBackedBytes { get; set; }

        public override string ToString()
        {
            string header = $"#{Pid} '{Name}'{(IsAccessDenied ? ", access denied" : "")}";
            string user = Uid.HasValue ? ($", Uid: {Uid}{(string.IsNullOrEmpty(UserName) ? "" : $" '{UserName}'")}") : "";
            string parentPid = ParentPid == 0 ? "" : $", {nameof(ParentPid)}: {ParentPid}";
            return $"{header}{user}{parentPid}, {nameof(StartAt)}: {StartAt}, {nameof(IoTime)}: {IoTime}, {nameof(UserCpuUsage)}: {UserCpuUsage}, {nameof(KernelCpuUsage)}: {KernelCpuUsage}, {nameof(RealTimePriority)}: {RealTimePriority}, {nameof(Priority)}: {Priority}, {nameof(MinorPageFaults)}: {MinorPageFaults}, {nameof(MajorPageFaults)}: {MajorPageFaults}, {nameof(NumThreads)}: {NumThreads}, {nameof(RssMem)}: {RssMem}, {nameof(PeakWorkingSet)}: {PeakWorkingSet}, {nameof(SharedMem)}: {SharedMem}, {nameof(SwappedMem)}: {SwappedMem}, {nameof(Command)}: {Command}, {nameof(ReadBytes)}: {ReadBytes}, {nameof(WriteBytes)}: {WriteBytes}, {nameof(ReadSysCalls)}: {ReadSysCalls}, {nameof(WriteSysCalls)}: {WriteSysCalls}, {nameof(ReadBlockBackedBytes)}: {ReadBlockBackedBytes}, {nameof(WriteBlockBackedBytes)}: {WriteBlockBackedBytes}";
        }

        public static ProcessIoStat[] GetProcesses()
        {
            Process[] builtIn = Process.GetProcesses();
            ProcessIoStat[] ret = new ProcessIoStat[builtIn.Length];
            int index = 0;
            foreach (var process in builtIn)
            {
                var ioInfo = new ProcessIoStat()
                {
                    Pid = process.Id,
                    Name = process.ProcessName,
                    PeakWorkingSet = process.PeakWorkingSet64, // Does not work on Linux
                };

                try
                {
                    ParseStat(ref ioInfo);
                    ParseStatus(ref ioInfo);
                    ParseIo(ref ioInfo);
                }
                catch (UnauthorizedAccessException)
                {
                    ioInfo.IsAccessDenied = true;
                }

                ret[index++] = ioInfo;
            }
            
            Dictionary<int,string> userNames = new Dictionary<int, string>();
            for(int i=0; i<ret.Length; i++)
            {
                var uid = ret[i].Uid;
                if (uid.HasValue)
                {
                    string userName;
                    if (!userNames.TryGetValue(uid.Value, out userName))
                    {
                        userName = GetNameByUid(uid.Value);
                        userNames[uid.Value] = userName;
                    }

                    ret[i].UserName = userName;
                }
            }

            return ret;
        }

        static string GetNameByUid_Legacy(int uid)
        {
            return Mono.Posix.Syscall.getusername(uid);
        }

        static string GetNameByUid(int uid)
        {
            var user = Mono.Unix.Native.Syscall.getpwuid((uint) uid);
            return user?.pw_name;
        }

        private static void ParseIo(ref ProcessIoStat ioStat)
        {
            var isFileName = $"/proc/{ioStat.Pid}/io";
            using (FileStream fs = new FileStream(isFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader rdr = new StreamReader(fs, Utf8Encoding))
            {
                int lookingFor = 6;
                string line;
                while (lookingFor > 0 && (line = rdr.ReadLine()) != null)
                {
                    if (line.StartsWith("rchar:", StringComparison.OrdinalIgnoreCase)) { ioStat.ReadBytes = GetIoValue(line); lookingFor--; }
                    if (line.StartsWith("wchar:", StringComparison.OrdinalIgnoreCase)) { ioStat.WriteBytes = GetIoValue(line); lookingFor--;}
                    if (line.StartsWith("syscr:", StringComparison.OrdinalIgnoreCase)) { ioStat.ReadSysCalls = GetIoValue(line); lookingFor--;}
                    if (line.StartsWith("syscw:", StringComparison.OrdinalIgnoreCase)) { ioStat.WriteSysCalls = GetIoValue(line); lookingFor--;}
                    if (line.StartsWith("read_bytes:", StringComparison.OrdinalIgnoreCase)) { ioStat.ReadBlockBackedBytes = GetIoValue(line); lookingFor--;}
                    if (line.StartsWith("write_bytes:", StringComparison.OrdinalIgnoreCase)) { ioStat.WriteBlockBackedBytes = GetIoValue(line); lookingFor--;}
                }
            }
        }

        private static void ParseStatus(ref ProcessIoStat ioStat)
        {
            var statusName = $"/proc/{ioStat.Pid}/status";
            if (!File.Exists(statusName)) return;
            using (FileStream fs = new FileStream(statusName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader rdr = new StreamReader(fs, Utf8Encoding))
            {
                int lookingFor = 6;
                long? VmRSS = null, RssFile = null, RssShmem = null, VmSwap = null;
                string line;
                while (lookingFor > 0 && (line = rdr.ReadLine()) != null)
                {
                    // Console.WriteLine($"{statusName}: {line}");
                    if (line.StartsWith("VmRSS:", StringComparison.OrdinalIgnoreCase)) { VmRSS = GetStatusValue(line); lookingFor--; }
                    if (line.StartsWith("RssFile:", StringComparison.OrdinalIgnoreCase)) { RssFile = GetStatusValue(line); lookingFor--;}
                    if (line.StartsWith("RssShmem:", StringComparison.OrdinalIgnoreCase)) { RssShmem = GetStatusValue(line); lookingFor--;}
                    if (line.StartsWith("VmSwap:", StringComparison.OrdinalIgnoreCase)) { VmSwap = GetStatusValue(line); lookingFor--;}
                    if (line.StartsWith("Uid:", StringComparison.OrdinalIgnoreCase)) { ioStat.Uid = GetRealUid(line); lookingFor--;}
                    if (line.StartsWith("PPid:", StringComparison.OrdinalIgnoreCase)) { ioStat.ParentPid = (int) GetStatusValue(line).GetValueOrDefault(); lookingFor--;}
                }

                if (VmRSS.HasValue) ioStat.RssMem = VmRSS.Value;
                if (VmSwap.HasValue) ioStat.SwappedMem = VmSwap.Value;
                if (RssFile.HasValue && RssShmem.HasValue) ioStat.SharedMem = RssFile.GetValueOrDefault() + RssShmem.GetValueOrDefault(); 
            }

        }

        private static long GetIoValue(string line)
        {
            return GetLong(line.Substring(line.IndexOf(':') + 1));
        }

        private static long? GetStatusValue(string line)
        {
            // TODO: Make Faster
            var sub = line.Substring(line.IndexOf(':') + 1).ToLower().Replace(" kb", "").Trim();
            return GetOptionalLong(sub);
        }

        private static int? GetRealUid(string line)
        {
            
            var sub = line.Substring(line.IndexOf(':') + 1);
            var uidsAsArr = sub.TrimStart().Split(' ', '\t');
            if (uidsAsArr.Length >= 4)
            {
                if (int.TryParse(uidsAsArr[0], out var realUid))
                {
                    return realUid;
                }
            }
            
            return null;
        }

        static readonly CultureInfo EnUS = new CultureInfo("en-US");
        private static readonly UTF8Encoding Utf8Encoding = new UTF8Encoding(false);

        static void ParseStat(ref ProcessIoStat ioStat)
        {
            var statName = $"/proc/{ioStat.Pid}/stat";
            if (!File.Exists(statName)) return;
            using (FileStream fs = new FileStream(statName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader rdr = new StreamReader(fs, Utf8Encoding))
            {
                var line = rdr.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    var arr = line.Split(' ');
                    ioStat.IoTime = GetLong(arr[42 - 1]);
                    ioStat.StartAt = GetLong(arr[22 - 1]);
                    ioStat.UserCpuUsage = GetLong(arr[14 - 1]);
                    ioStat.KernelCpuUsage = GetLong(arr[15 - 1]);
                    ioStat.RealTimePriority = GetLong(arr[18 - 1]);
                    ioStat.Priority = GetLong(arr[19 - 1]);
                    ioStat.MinorPageFaults = GetLong(arr[10 - 1]);
                    ioStat.MajorPageFaults = GetLong(arr[12 - 1]);
                    ioStat.NumThreads = GetLong(arr[20 - 1]);
                }
            }
        }

        static long? GetOptionalLong(string raw)
        {
            if (long.TryParse(raw, NumberStyles.Number, EnUS, out long ret))
                return ret;

            return null;
        }

        static long GetLong(string raw)
        {
            if (long.TryParse(raw, NumberStyles.Number, EnUS, out long ret))
                return ret;

            throw new Exception($"Invalid long '{raw}'");
        }

    }
}