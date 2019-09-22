using System;
using System.Collections.Generic;
using System.Linq;
using KernelManagementJam.DebugUtils;
using NUnit.Framework;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class MountsTests
    {
        [Test]
        public void Root_FileSystem_Exists()
        {
            bool isLinux = Environment.OSVersion.Platform == PlatformID.Unix;
            if (!isLinux)
            {
                Console.WriteLine("Warning! Test Ignored. Linux is required");
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
            Assert.Greater(root2.FreeSpace, 0, "FreeSpace of the '/' is positive number, except of the readonly root filesystem");
            Assert.Greater(root2.TotalSize, 0, "TotalSize of the '/' is positive number");
            
            Assert.IsNotNull(root2.MountEntry?.Device, "MountEntry.Device of the '/' is not null");
            Assert.IsNotEmpty(root2.MountEntry?.Device, "MountEntry.Device of the '/' is not empty");
            
            Assert.IsNotNull(root2.MountEntry?.FileSystem, "MountEntry.FileSystem of the '/' is not null");
            Assert.IsNotEmpty(root2.MountEntry?.FileSystem, "MountEntry.FileSystem of the '/' is not empty");
            
            
            
            // analyz.Details;
            return;
        }
        
    }
}