namespace KernelManagementJam.Tests
{
    public class ProcCpuInfoTestCase
    {
        public readonly string Title, ProcCpuInfo;

        public ProcCpuInfoTestCase(string title, string procCpuInfo)
        {
            Title = title;
            ProcCpuInfo = procCpuInfo;
        }

        public override string ToString()
        {
            return Title;
        }

        public static ProcCpuInfoTestCase[] AllCases => new ProcCpuInfoTestCase[]
        {
            new ProcCpuInfoTestCase("Helio G99", ProcCpuInfoCopiesRaw.HelioG99),
            new ProcCpuInfoTestCase("Snapdragon 835", ProcCpuInfoCopiesRaw.Snapdragon835),
            new ProcCpuInfoTestCase("RK3399", ProcCpuInfoCopiesRaw.RK3399),
            new ProcCpuInfoTestCase("AllWinner H3", ProcCpuInfoCopiesRaw.AllWinnerH3),
            new ProcCpuInfoTestCase("BCM2711 RPI4", ProcCpuInfoCopiesRaw.RaspberryPi4),
            new ProcCpuInfoTestCase("BCM2712 RPI5", ProcCpuInfoCopiesRaw.RaspberryPi5),
            new ProcCpuInfoTestCase("Orange PI 5", ProcCpuInfoCopiesRaw.OrangePi5),
        };
    }
}
