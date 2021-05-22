using System;
using System.Globalization;

namespace Universe.FioStream
{
    public partial class FioStreamReader
    {
        private static readonly CultureInfo EnUs = new CultureInfo("en-US");

        private static readonly StringComparison IgnoreCaseComparision = 
#if NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_6 || NETCOREAPP1_1 || NETCOREAPP1_0  
            StringComparison.OrdinalIgnoreCase;
#else
            StringComparison.OrdinalIgnoreCase;
#endif

        
        private static double? TryParseIops(string arg)
        {
            return TryParseHumanDouble(arg);
        }

        private static double? TryParseBandwidth(string arg)
        {
            if (arg.EndsWith("/s", IgnoreCaseComparision) && arg.Length >= 3)
                arg = arg.Substring(0, arg.Length - 2);

            return TryParseHumanDouble(arg);
        }

        private static double? TryParseHumanDouble(string arg)
        {
            var original = arg;
            
            var suffixes = HumanSizeSuffix.All;
            long scale = 1;
            foreach (var suffix in suffixes)
            {
                if (arg.EndsWith(suffix.Suffix, IgnoreCaseComparision) && arg.Length >= suffix.SuffixLength + 1)
                {
                    arg = arg.Substring(0, arg.Length - suffix.Suffix.Length);
                    scale = suffix.Scale;
                    break;
                }
            }

            if (double.TryParse(arg, NumberStyles.AllowDecimalPoint, EnUs, out var ret))
            {
                return ret * scale;
            }
            
#if DEBUG
            throw new ArgumentException($"Invalid bandwidth/IOPS argument [{original}]", arg);
#endif
            return null;
        }

        
    }
}