using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KernelManagementJam
{
    // fan[1-*]_
    // temp[1-*]_
    public class LinuxHwmonSensor
    {
        // hwmon*/name
        public string Name { get; set; }
        
        // hwmon{index}
        public int? Index { get; set; }
        public List<LinuxHwmonSensorInput> Inputs { get; }

        public LinuxHwmonSensor()
        {
            Index = null;
            Inputs = new List<LinuxHwmonSensorInput>();
        }
    }
    
    public class LinuxHwmonSensorInput
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public LinuxHwmonSensorKind Kind { get; set; }
        public string Label { get; set; }
        public int Index { get; set; }
        public int Value { get; set; }
    }

    public enum LinuxHwmonSensorKind
    {
        Fan,
        Temperature,
        Pwm,
        Current,
        Voltage,
    }

}
