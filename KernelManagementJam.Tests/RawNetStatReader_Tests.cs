using System.IO;
using System.Text;
using NUnit.Framework;
using Universe.NUnitTests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class RawNetStatReader_Tests : NUnitTestsBase
    {
        [Test]
        public void ReadRawNetStat()
        {
            var reader = new RawNetStatReader(new StringReader(GetRawNetStat()));
            CollectionAssert.IsNotEmpty(reader.NetStatItems);
            
        }
        
        static string GetRawNetStat()
        {
            var file = "/proc/net/netstat";
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rdr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                return rdr.ReadToEnd();
            }
        }

    }
}
