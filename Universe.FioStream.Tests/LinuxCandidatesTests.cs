using System;
using System.Linq;
using NUnit.Framework;
using Tests;
using Universe.FioStream.Binaries;

namespace Universe.FioStream.Tests
{
    [TestFixture]
    public class LinuxCandidatesTests : NUnitTestsBase
    {
        
        private static string[] Machines => "i386 i686 x86_64 armv7l armv5tejl armv6l armv7l aarch64 ppc ppc64le mips64".Split();

        [Test]
        [TestCaseSource(typeof(LinuxCandidatesTests), nameof(LinuxCandidatesTests.Machines))]
        public void Show(string machine)
        {
            var candidates = OrderedLinuxCandidates.FindCandidateByLinuxMachine(machine);
            Console.WriteLine($"Machine: {machine}, candidates: {candidates.Count()}");
            foreach (var candidate in candidates)
            {
                Console.WriteLine(candidate);
            }
        }
        
    }
}