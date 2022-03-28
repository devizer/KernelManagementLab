using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Universe;
using Universe.NUnitTests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class NetDevParser_Tests: NUnitTestsBase
    {
        private static readonly UTF8Encoding Utf8Encoding = new UTF8Encoding(false);

        [Test]
        public void Does_Work_and_Provides_Data()
        {
            if (CrossInfo.ThePlatform != CrossInfo.Platform.Linux) return;
            
            using(FileStream fs = new FileStream("/proc/net/dev", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader rdr = new StreamReader(fs, Utf8Encoding))
                {
                    NetDevParser netDevParser = new NetDevParser(rdr);
                    var interfaces = netDevParser.Interfaces;
                    CollectionAssert.IsNotEmpty(interfaces);
                    Assert.IsTrue(interfaces.Any(x => !x.IsInactive));
                    Console.WriteLine($"Total bytes sent+received {interfaces.Sum(x => x.TxBytes + x.RxBytes):n0}");
                    
                }
            }

        }
    }
}
