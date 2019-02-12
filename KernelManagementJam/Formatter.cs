namespace KernelManagementJam
{
    public static class Formatter
    {
        public static string FormatBytes(long number)
        {
            if (number == 0)
                return "0";

            if (number < 9999)
                return number.ToString("0") + "B";

            if (number < 9999999)
                return (number / 1024d).ToString("0.#") + "K";

            if (number < 9999999999)
                return (number / 1024d / 1024d).ToString("0.#") + "M";

            if (number < 9999999999999)
                return (number / 1024d / 1024d).ToString("0.#") + "M";

            return (number / 1024d / 1024d / 1024d / 1024d).ToString("0.#") + "T";
        }
    }
}