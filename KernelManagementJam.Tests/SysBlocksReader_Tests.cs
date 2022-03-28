using System;
using System.Linq;
using System.Text;
using KernelManagementJam.DebugUtils;
using NUnit.Framework;
using Universe;
using Universe.NUnitTests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class SysBlocksReader_Tests : NUnitTestsBase
    {
        private static readonly UTF8Encoding Utf8Encoding = new UTF8Encoding(false);

        [Test]
        public void Does_Work_and_Provides_Data()
        {
            if (CrossInfo.ThePlatform != CrossInfo.Platform.Linux) return;
            var snapshot = SysBlocksReader.GetSnapshot(new AdvancedMiniProfilerKeyPath("Test SysBlocksReader"));
            CollectionAssert.IsNotEmpty(snapshot);
            var info = snapshot.Select(d => $" --- {d.DevFileType}: {d.DiskKey} ({string.Join(",", d.Volumes.Select(v =>v.VolumeKey))})");
            // var info = snapshot.Select(d => $" --- {d.DevFileType}: {d.DiskKey}");
            Console.WriteLine(string.Join(Environment.NewLine, info));
            Console.WriteLine(snapshot.AsJson());
        }
    }
}
