namespace MountLab
{
    static class Formatter
    {
        public static string FormatBytes(long number)
        {
            if (number == 0)
                return "0";
            else if (number < 9999)
                return number.ToString("0") + "B";
            else if (number < 9999999)
                return (number / 1024d).ToString("0.#") + "K";
            else if (number < 9999999999)
                return (number / 1024d / 1024d).ToString("0.#") + "M";
            else 
                return (number / 1024d / 1024d / 1024d).ToString("0.#") + "G";
        }
    }
}