namespace MountLab
{
    static class Formatter
    {
        public static string FormatBytes(long number)
        {
            if (number == 0)
                return "0";
            else if (number < 9999)
                return number.ToString("n0") + "B";
            else if (number < 9999999)
                return (number / 1024d).ToString("n1") + "K";
            else if (number < 9999999999)
                return (number / 1024d / 1024d).ToString("n1") + "M";
            else 
                return (number / 1024d / 1024d / 1024d).ToString("n1") + "G";
        }
    }
}