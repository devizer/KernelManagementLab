using System;
using System.Collections.Generic;
using System.Linq;
using KernelManagementJam;

namespace Universe.Dashboard.Agent
{
    public class CpuTemperatureDataSource
    {
        public static List<LinuxHwmonSensor> Instance = new List<LinuxHwmonSensor>();
    }

    public static class LinuxHwmonSensorExtensions
    {
        public static string ToShortCpuTemperatureHtmlInfo(this IEnumerable<LinuxHwmonSensor> sensors, bool needFahrenheit = false)
        {
            // "Name": "cpu_thermal", or
            var ignore = StringComparison.OrdinalIgnoreCase;
            int? temperature = null;
            foreach (var sensor in sensors)
            {
                if (sensor.Name == "cpu_thermal")
                {
                    temperature = sensor
                        .Inputs
                        .FirstOrDefault(x => x.Kind == LinuxHwmonSensorKind.Temperature && x.Value > 0)
                        ?.Value;
                }

                else if (sensor.Name?.StartsWith("coretemp", ignore) == true)
                {
                    temperature = sensor
                        .Inputs
                        .Where(x => x.Kind == LinuxHwmonSensorKind.Temperature && x.Value > 0)
                        .Max(x => x.Value);
                }

                else if (sensor.Name?.IndexOf("k8temp", ignore) >= 0 || sensor.Name?.IndexOf("k10temp", ignore) >= 0)
                {
                    // Same as coretemp
                    temperature = sensor
                        .Inputs
                        .Where(x => x.Kind == LinuxHwmonSensorKind.Temperature && x.Value > 0)
                        .Max(x => x.Value);
                }

                if (temperature.GetValueOrDefault() > 0) break;
            }

            if (!temperature.HasValue)
                return null;

            return $"{(temperature.Value / 1000f):f0} ℃";
        }
        
    }
}
