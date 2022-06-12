using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KernelManagementJam
{
    public class LinuxHwmonParser
    {
        // HwmonSensor  /sys/class/hwmon/hwmon*
        public static IEnumerable<LinuxHwmonSensor> GetAll()
        {
            List<LinuxHwmonSensor> ret = new List<LinuxHwmonSensor>();
            
            var hwmonDirs = new DirectoryInfo("/sys/class/hwmon").GetDirectories("hwmon*");
            foreach (var hwmonDir in hwmonDirs)
            {
                LinuxHwmonSensor hwmonSensor = new LinuxHwmonSensor();
                
                // hwmonSensor.Index
                var hwmonDirName = hwmonDir.Name;
                if (int.TryParse(hwmonDirName.Substring("hwmon".Length), out var rawHwMonDirIndex))
                    hwmonSensor.Index = rawHwMonDirIndex;
                
                var files = hwmonDir.GetFiles("*");

                if (files.Any(x => x.Name == "name"))
                    hwmonSensor.Name = SmallFileReader.ReadFirstLine(Path.Combine(hwmonDir.FullName, "name"));

                var sensorKindInfoList = new[]
                {
                    new {Kind = LinuxHwmonSensorKind.Fan, Prefix = "fan"},
                    new {Kind = LinuxHwmonSensorKind.Temperature, Prefix = "temp"},
                };

                foreach (var kindInfo in sensorKindInfoList)
                {
                    var inputFiles = files.Select(x => x.Name).Where(x => x.StartsWith(kindInfo.Prefix));
                    var inputFilesLabels = inputFiles.Where(x => x.EndsWith("_label")).ToArray();
                    var inputFilesValues = inputFiles.Where(x => x.EndsWith("_input")).ToArray();
                    foreach (string inputValueFile in inputFilesValues)
                    {
                        int? valueIndex = TryParseValueIndex(inputValueFile, kindInfo.Prefix);
                        if (valueIndex.HasValue)
                        {
                            var labelFile = $"{kindInfo.Prefix}{valueIndex.Value:0}_label";
                            if (!inputFilesLabels.Contains(labelFile)) continue;

                            var valueFile = Path.Combine(hwmonDir.FullName, inputValueFile);
                            // Console.WriteLine($"INDEX=[{valueIndex.Value}] LABEL=[{labelFile}] INPUT=[{valueFile}]");
                            var rawValue = SmallFileReader.ReadFirstLine(valueFile);
                            if (int.TryParse(rawValue, out var value))
                            {
                                var label = SmallFileReader.ReadFirstLine(Path.Combine(hwmonDir.FullName, labelFile));
                                if (!string.IsNullOrEmpty(labelFile))
                                {
                                    LinuxHwmonSensorInput input = new LinuxHwmonSensorInput()
                                    {
                                        Kind = kindInfo.Kind,
                                        Index = valueIndex.Value,
                                        Value = value,
                                        Label = label,
                                    };
                                    hwmonSensor.Inputs.Add(input);
                                }
                            }
                        }
                    }
                }
                
                hwmonSensor.Inputs.Sort((left, right) => (left?.Index ?? 0).CompareTo(right?.Index ?? 0));
                ret.Add(hwmonSensor);
            }

            return ret.OrderBy(x => x.Index).ToList();
        }

        private static int? TryParseValueIndex(string inputValueFile, string prefix)
        {
            if (inputValueFile.Length > prefix.Length + 1)
            {
                var nameWithoutPrefix = inputValueFile.Substring(prefix.Length);
                int underPos = nameWithoutPrefix.IndexOf('_');
                if (underPos > 0)
                {
                    if (int.TryParse(nameWithoutPrefix.Substring(0, underPos), out var valueIndex))
                    {
                        // Console.WriteLine($"FOR [{inputValueFile}] index is {valueIndex}");
                        return valueIndex;
                    }
                        
                }
            }
            return null;
        }

    }
}
