using System;
using System.Globalization;

namespace Universe.FioStream
{
    public partial class FioStreamReader
    {
        private static readonly CultureInfo EnUs = CultureInfo.InvariantCulture; // new CultureInfo("en-US");

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
            if (arg.Length >= 3 && arg.EndsWith("/s", IgnoreCaseComparision))
                arg = arg.Substring(0, arg.Length - 2);

            return TryParseHumanDouble(arg);
        }

        static double[] DoublePowersOf10 = { 1d, 10, 100, 
            1000, 
            10000, 
            100000, 
            1000000, 
            10000000, 
            100000000,
            1000000000,
            10000000000,
            100000000000,
            1000000000000,
            10000000000000,
            100000000000000,
            1000000000000000,
            10000000000000000,
            100000000000000000,
            1000000000000000000,
        };
        
        static bool TryParseDouble(string input, out double result)
        {
            result = default(double);
            long n = 0;
            int length = input.Length; 
            int decimalPosition = length;
            bool hasDecimalPosition = false;
            for (int k = 0; k < input.Length; k++) { 
                char c = input[k];
                if (c == '.')
                {
                    if (hasDecimalPosition) return false;
                    decimalPosition = k + 1;
                    hasDecimalPosition = true;
                }
                else if (c < '0' || c > '9')
                {
                    return false;
                }
                else
                {
                    n = n * 10 + (c - '0');
                }
            }

            var powerIndex = input.Length - decimalPosition;
            if (powerIndex >= DoublePowersOf10.Length) return false;
            
            result = n / DoublePowersOf10[powerIndex];
            return true;
        }

        private static double? TryParseHumanDouble(string arg)
        {
            var original = arg;
            
            var suffixes = HumanSizeSuffix.All;
            long scale = 1;
            foreach (var suffix in suffixes)
            {
                if (arg.Length >= suffix.SuffixLength + 1 && arg.EndsWith(suffix.Suffix, IgnoreCaseComparision))
                {
                    arg = arg.Substring(0, arg.Length - suffix.Suffix.Length);
                    scale = suffix.Scale;
                    break;
                }
            }

            /*
            if (double.TryParse(arg, NumberStyles.AllowDecimalPoint, EnUs, out var ret))
            {
                return ret * scale;
            }
            */
            if (TryParseDouble(arg.Trim(), out var ret))
            {
                return ret * scale;
            }
            
#if DEBUG
            throw new ArgumentException($"Invalid bandwidth/IOPS argument [{original}]", arg);
#endif
            return null;
        }

        double? TryParsePercents(string arg)
        {
            if (arg.Length > 1 && arg[arg.Length - 1] == '%')
                arg = arg.Substring(0, arg.Length - 1);
            
            if (double.TryParse(arg, NumberStyles.AllowDecimalPoint, EnUs, out var ret))
            {
                return ret;
            }
            
#if DEBUG
            throw new ArgumentException($"Invalid per cents argument [{arg}]", arg);
#endif
            return null;

        }

    }
}