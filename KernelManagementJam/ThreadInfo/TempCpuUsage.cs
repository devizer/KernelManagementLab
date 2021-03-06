using System.Dynamic;
using System.Runtime.InteropServices;

namespace KernelManagementJam.ThreadInfo
{
    // Supported by kernel 2.6.26+ and mac os 10.9+, Windows XP/2003 and above
    [StructLayout(LayoutKind.Sequential)]
    public struct TempCpuUsage
    {
        public TimeValue UserUsage;
        public TimeValue KernelUsage;

        public override string ToString()
        {
            return $"User: {UserUsage}, Kernel: {KernelUsage}";
        }

        public static TempCpuUsage Substruct(TempCpuUsage onEnd, TempCpuUsage onStart)
        {
            var user = onEnd.UserUsage.TotalMicroSeconds - onStart.UserUsage.TotalMicroSeconds;
            var system = onEnd.KernelUsage.TotalMicroSeconds - onStart.KernelUsage.TotalMicroSeconds;
            const long _1M = 1000000L;
            return new TempCpuUsage()
            {
                UserUsage = new TimeValue() {Seconds = user / _1M, MicroSeconds = user % _1M},
                KernelUsage = new TimeValue() {Seconds = system / _1M, MicroSeconds = system % _1M},
            };
        }
    }
    

    [StructLayout(LayoutKind.Sequential)] 
    public struct TimeValue
    {
        public long Seconds;
        public long MicroSeconds;

        public long TotalMicroSeconds => Seconds * 1000000 + MicroSeconds;
        public double TotalSeconds => Seconds + MicroSeconds / 1000000d;


        public override string ToString()
        {
            return $"{TotalMicroSeconds / 1000d:n3} milliseconds";
        }
    }
    
    public enum CpuUsageScope
    {
        Thread, 
        Process,  
    }

}