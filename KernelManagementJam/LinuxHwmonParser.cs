using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KernelManagementJam
{
    public class LinuxHwmonParser
    {
        // HwmonSensor  /sys/class/hwmon/hwmon*
        public static IEnumerable<HwmonSensor> GetAll()
        {
            var hwmonDirs = new DirectoryInfo("/sys/class/hwmon").GetDirectories("hwmon*");
            foreach (var hwmonDir in hwmonDirs)
            {
                var files = hwmonDir.GetFiles("*");

                string name = null;
                if (files.Any(x => x.Name == "name"))
                    name = SmallFileReader.ReadFirstLine(Path.Combine(hwmonDir.FullName, "name"));
            }
            
            throw new NotImplementedException();
        }
        
    }
    
    // fan[1-*]_
    // temp[1-*]_
    public class HwmonSensor
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public IEnumerable<HwmonSensorInput> Inputs { get; set; }
    }

    public class HwmonSensorInput
    {
        private HwmonSensorKind Kind { get; set; }
        public string Label { get; set; }
        public int Index { get; set; }
        public int Value { get; set; }
    }

    public enum HwmonSensorKind
    {
        Fan,
        Temperature,
        Pwm,
        Current,
        Voltage,
    }
}
