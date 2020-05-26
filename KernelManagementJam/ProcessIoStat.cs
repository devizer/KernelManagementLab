using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Unix.Native;

namespace KernelManagementJam
{
    public struct ProcessIoStat
    {
        public int Pid { get; set; } // 1, first
        public ProcessKind Kind { get; set; }
        public int ParentPid { get; set; } // 1, first
        public bool IsAccessDenied { get; set; }
        public bool IsZombie { get; set; }
        public long StartAtRaw { get; set; } // 22
        public double StartAt => StartAtRaw / _Sc_Clk_Tck.Value;

        public int? Uid { get; set; }
        public string UserName { get; set; }

        public double IoTime { get; set; } // 42
        public double UserCpuUsage { get; set; }   // 14
        public double KernelCpuUsage { get; set; } // 15
        public double GuestTime { get; set; }        // 43
        
        public double ChildrenUserCpuUsage { get; set; } // 16
        public double ChildrenKernelCpuUsage { get; set; } // 17
        public double ChildrenGuestTime { get; set; } // 44
        public long SchedulingPolicy { get; set; } // 41
        
        // Nice: 0...39 is -20..19
        // Realtime: -2 ==> 1, -3 ==> 2, ... -100 ==> 99 
        public long MixedPriority { get; set; } // 18
        
        // 0 - non-realtime, otherwise 1..99
        public long RealtimePriority { get; set; } // 40
        
        public long Nice { get; set; } // 19 (nice)
        public long MinorPageFaults { get; set; } // 10
        public long MajorPageFaults { get; set; } // 12
        public long ChildrenMinorPageFaults { get; set; } // 11
        public long ChildrenMajorPageFaults { get; set; } // 13
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
            string priority = MixedPriority >= 0 && MixedPriority <= 39
                ? $"Nice {(MixedPriority - 20)}"
                : MixedPriority >= -100 && MixedPriority <= -2
                    ? $"RT {(-MixedPriority - 1)}"
                    : $"{MixedPriority}";

            if (IsZombie)
                return $"{header}, {priority}{user}{parentPid}, zombie";
            
            return $"{header}, {priority}{user}{parentPid}, {nameof(Kind)}: {Kind}, {nameof(StartAtRaw)}: {StartAtRaw}, {nameof(IoTime)}: {IoTime}, CpuUsage: {UserCpuUsage} (user) + {KernelCpuUsage} (kernel), Children CpuUsage: {ChildrenUserCpuUsage} (user) + {ChildrenKernelCpuUsage} (kernel), GuestTime: {GuestTime} (own) + {ChildrenGuestTime} (Children) {nameof(SchedulingPolicy)}: {SchedulingPolicy}, {nameof(MixedPriority)}: {MixedPriority}, {nameof(RealtimePriority)}: {RealtimePriority}, {nameof(Nice)}: {Nice}, PageFaults: {MinorPageFaults} (minor) + {MajorPageFaults} (major), Children PageFaults: {ChildrenMinorPageFaults} (minor) + {ChildrenMajorPageFaults} (major), {nameof(NumThreads)}: {NumThreads}, {nameof(RssMem)}: {RssMem}, {nameof(PeakWorkingSet)}: {PeakWorkingSet}, {nameof(SharedMem)}: {SharedMem}, {nameof(SwappedMem)}: {SwappedMem}, {nameof(Command)}: {Command}, {nameof(ReadBytes)}: {ReadBytes}, {nameof(WriteBytes)}: {WriteBytes}, {nameof(ReadSysCalls)}: {ReadSysCalls}, {nameof(WriteSysCalls)}: {WriteSysCalls}, {nameof(ReadBlockBackedBytes)}: {ReadBlockBackedBytes}, {nameof(WriteBlockBackedBytes)}: {WriteBlockBackedBytes}";
        }

        public static ProcessIoStat GetByProcessId(int pid)
        {
            var process = Process.GetProcessById(pid);
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
            catch (System.IO.DirectoryNotFoundException)
            {
                ioInfo.IsZombie = true;
            }
            catch (UnauthorizedAccessException)
            {
                ioInfo.IsAccessDenied = true;
            }

            if (ioInfo.Uid.HasValue)
                ioInfo.UserName = GetNameByUid(ioInfo.Uid.Value);

            return ioInfo;
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
                catch (System.IO.DirectoryNotFoundException)
                {
                    ioInfo.IsZombie = true;
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

            FullFillKind(ret);

            return ret;
        }

        private static void FullFillKind(ProcessIoStat[] ret)
        {
            const string containerd_shim = "containerd-shim", containerd = "containerd"; 
            var byId = ret.ToDictionary(x => x.Pid);
            for(int i=0, n=ret.Length; i<n; i++)
            {
                if (ret[i].IsZombie) continue;
                else if (ret[i].Pid == 1) ret[i].Kind = ProcessKind.Init;
                else if (ret[i].ParentPid == 1) ret[i].Kind = ProcessKind.Service;
                else
                {
                    if (byId.TryGetValue(ret[i].ParentPid, out var parent1))
                    {
                        if (parent1.Name == containerd_shim)
                        {
                            if (byId.TryGetValue(parent1.ParentPid, out var parent2))
                            {
                                if (parent2.ParentPid == 1 && parent2.Name == containerd)
                                {
                                    ret[i].Kind = ProcessKind.Container;
                                }
                            }
                        }
                    }
                }
                
            }
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
                    var ticksPerSecond = (double) _Sc_Clk_Tck.Value;
                    var arr = line.Split(' ');
                    if (string.IsNullOrEmpty(ioStat.Name)) ioStat.Name = arr[2 - 1];
                    ioStat.IoTime = GetLong(arr[42 - 1]) / ticksPerSecond;
                    ioStat.StartAtRaw = GetLong(arr[22 - 1]); // divide by sysconf(_SC_CLK_TCK)
                    ioStat.UserCpuUsage = GetLong(arr[14 - 1]) / ticksPerSecond;
                    ioStat.KernelCpuUsage = GetLong(arr[15 - 1]) / ticksPerSecond;

                    ioStat.SchedulingPolicy = GetLong(arr[41 - 1]);
                    ioStat.MixedPriority = GetLong(arr[18 - 1]);
                    ioStat.RealtimePriority = GetLong(arr[40 - 1]);
                    ioStat.Nice = GetLong(arr[19 - 1]);
                    
                    ioStat.MinorPageFaults = GetLong(arr[10 - 1]);
                    ioStat.MajorPageFaults = GetLong(arr[12 - 1]);
                    ioStat.NumThreads = GetLong(arr[20 - 1]);
                    
                    // 16 - children user mode time
                    ioStat.ChildrenUserCpuUsage = GetLong(arr[16 - 1]) / ticksPerSecond;
                    // 17 - children kernel mode time
                    ioStat.ChildrenKernelCpuUsage = GetLong(arr[17 - 1]) / ticksPerSecond;
                    // 11 - Minor page faults for children
                    ioStat.ChildrenMinorPageFaults = GetLong(arr[11 - 1]);
                    // 13 - Major faults for children
                    ioStat.ChildrenMajorPageFaults = GetLong(arr[13 - 1]);
                    
                    ioStat.GuestTime = GetLong(arr[43 - 1]) / ticksPerSecond;
                    ioStat.ChildrenGuestTime = GetLong(arr[44 - 1]) / ticksPerSecond;
                    
                }
            }
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
                    else if (line.StartsWith("wchar:", StringComparison.OrdinalIgnoreCase)) { ioStat.WriteBytes = GetIoValue(line); lookingFor--;}
                    else if (line.StartsWith("syscr:", StringComparison.OrdinalIgnoreCase)) { ioStat.ReadSysCalls = GetIoValue(line); lookingFor--;}
                    else if (line.StartsWith("syscw:", StringComparison.OrdinalIgnoreCase)) { ioStat.WriteSysCalls = GetIoValue(line); lookingFor--;}
                    else if (line.StartsWith("read_bytes:", StringComparison.OrdinalIgnoreCase)) { ioStat.ReadBlockBackedBytes = GetIoValue(line); lookingFor--;}
                    else if (line.StartsWith("write_bytes:", StringComparison.OrdinalIgnoreCase)) { ioStat.WriteBlockBackedBytes = GetIoValue(line); lookingFor--;}
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
                    else if (line.StartsWith("RssFile:", StringComparison.OrdinalIgnoreCase)) { RssFile = GetStatusValue(line); lookingFor--;}
                    else if (line.StartsWith("RssShmem:", StringComparison.OrdinalIgnoreCase)) { RssShmem = GetStatusValue(line); lookingFor--;}
                    else if (line.StartsWith("VmSwap:", StringComparison.OrdinalIgnoreCase)) { VmSwap = GetStatusValue(line); lookingFor--;}
                    else if (line.StartsWith("Uid:", StringComparison.OrdinalIgnoreCase)) { ioStat.Uid = GetRealUid(line); lookingFor--;}
                    else if (line.StartsWith("PPid:", StringComparison.OrdinalIgnoreCase)) { ioStat.ParentPid = (int) GetStatusValue(line).GetValueOrDefault(); lookingFor--;}
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


        private static Lazy<long> _Sc_Clk_Tck = new Lazy<long>(() =>
        {
            // TODO: check errors
            return Syscall.sysconf(SysconfName._SC_CLK_TCK);
        });

        public enum ProcessKind
        {
            Undefined,
            Init,
            Service,
            Container,
        }
    }
}