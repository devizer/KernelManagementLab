using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KernelManagementJam.DebugUtils;
using NUnit.Framework;
using Universe.NUnitTests;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class ProcMountsParser_Tests : NUnitTestsBase
    {
        [Test]
        public void Root_FileSystem_Exists()
        {
            bool isLinux = Environment.OSVersion.Platform == PlatformID.Unix;
            if (!isLinux || !File.Exists("/proc/mounts"))
            {
                Console.WriteLine("Warning! Test Ignored. Linux and the /proc/mounts file are required");
                return;
            }
            
            IList<MountEntry> mounts = ProcMountsParser.Parse("/proc/mounts").Entries;
            var root1 = mounts.FirstOrDefault(x => x.MountPath == "/");
            Assert.IsNotNull(root1, "ProcMountsParser.Parse: MountPath == '/' exists");

            ProcMountsAnalyzer analyz = ProcMountsAnalyzer.Create(mounts, skipDetailsLog: true);
            var root2 = analyz.Details.FirstOrDefault(x => x.MountEntry.MountPath == "/");
            Assert.IsNotNull(root2, "ProcMountsAnalyzer.Create: MountPath == '/' exists");
            
            Console.WriteLine($"The Root Volume:{Environment.NewLine}{root2.AsJson()}");
            
            // we never run tests on the readonly root
            if (!root2.IsReadonly == false)
                Assert.Greater(root2.FreeSpace, 0, "FreeSpace of the '/' is positive number (tests on readonly root filesystem is nonsense");
            
            Assert.Greater(root2.TotalSize, 0, "TotalSize of the '/' is positive number");
            
            Assert.IsNotNull(root2.MountEntry?.Device, "MountEntry.Device of the '/' is not null");
            Assert.IsNotEmpty(root2.MountEntry?.Device, "MountEntry.Device of the '/' is not empty");
            
            Assert.IsNotNull(root2.MountEntry?.FileSystem, "MountEntry.FileSystem of the '/' is not null");
            Assert.IsNotEmpty(root2.MountEntry?.FileSystem, "MountEntry.FileSystem of the '/' is not empty");


            ProcMountsAnalyzer analizer = ProcMountsAnalyzer.Create(mounts);
            CollectionAssert.IsNotEmpty(analizer.Details, "analizer.Details is not empty");
            var forBenchmark = analizer.Details.FilterForBenchmark();
            Console.WriteLine($"Drives for benchmark: {forBenchmark.Count()}{Environment.NewLine}{forBenchmark.AsJson()}");
            var forHuman = analizer.Details.FilterForHuman();
            Console.WriteLine($"Drives for a human: {forHuman.Count()}{Environment.NewLine}{forHuman.AsJson()}");

            foreach (DriveDetails drive in analizer.Details)
            {
                var json = drive.AsJson();
            }

        }
        
    }

}
