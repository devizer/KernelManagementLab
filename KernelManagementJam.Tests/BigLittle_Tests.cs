using NUnit.Framework;

namespace KernelManagementJam.Tests
{
    [TestFixture]
    public class BigLittle_Tests
    {
        [Test]
        public void ProcessorName()
        {
            
        }

        private static readonly string RK3399_Raw = @"
processor	: 0
model name	: ARMv8 Processor rev 4 (v8l)
BogoMIPS	: 48.00
Features	: half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt lpae evtstrm aes pmull sha1 sha2 crc32
CPU implementer	: 0x41
CPU architecture: 8
CPU variant	: 0x0
CPU part	: 0xd03
CPU revision	: 4

processor	: 1
model name	: ARMv8 Processor rev 4 (v8l)
BogoMIPS	: 48.00
Features	: half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt lpae evtstrm aes pmull sha1 sha2 crc32
CPU implementer	: 0x41
CPU architecture: 8
CPU variant	: 0x0
CPU part	: 0xd03
CPU revision	: 4

processor	: 2
model name	: ARMv8 Processor rev 4 (v8l)
BogoMIPS	: 48.00
Features	: half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt lpae evtstrm aes pmull sha1 sha2 crc32
CPU implementer	: 0x41
CPU architecture: 8
CPU variant	: 0x0
CPU part	: 0xd03
CPU revision	: 4

processor	: 3
model name	: ARMv8 Processor rev 4 (v8l)
BogoMIPS	: 48.00
Features	: half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt lpae evtstrm aes pmull sha1 sha2 crc32
CPU implementer	: 0x41
CPU architecture: 8
CPU variant	: 0x0
CPU part	: 0xd03
CPU revision	: 4

processor	: 4
model name	: ARMv8 Processor rev 2 (v8l)
BogoMIPS	: 48.00
Features	: half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt lpae evtstrm aes pmull sha1 sha2 crc32
CPU implementer	: 0x41
CPU architecture: 8
CPU variant	: 0x0
CPU part	: 0xd08
CPU revision	: 2

processor	: 5
model name	: ARMv8 Processor rev 2 (v8l)
BogoMIPS	: 48.00
Features	: half thumb fastmult vfp edsp neon vfpv3 tls vfpv4 idiva idivt lpae evtstrm aes pmull sha1 sha2 crc32
CPU implementer	: 0x41
CPU architecture: 8
CPU variant	: 0x0
CPU part	: 0xd08
CPU revision	: 2

Serial		: 0000000000000000";

        private static readonly string Snapdragon835_Raw = @"
Processor       : AArch64 Processor rev 4 (aarch64)
processor       : 0
BogoMIPS        : 38.40
Features        : fp asimd evtstrm aes pmull sha1 sha2 crc32
CPU implementer : 0x51
CPU architecture: 8
CPU variant     : 0xa
CPU part        : 0x801
CPU revision    : 4

processor       : 1
BogoMIPS        : 38.40
Features        : fp asimd evtstrm aes pmull sha1 sha2 crc32
CPU implementer : 0x51
CPU architecture: 8
CPU variant     : 0xa
CPU part        : 0x801
CPU revision    : 4

processor       : 2
BogoMIPS        : 38.40
Features        : fp asimd evtstrm aes pmull sha1 sha2 crc32
CPU implementer : 0x51
CPU architecture: 8
CPU variant     : 0xa
CPU part        : 0x801
CPU revision    : 4

processor       : 3
BogoMIPS        : 38.40
Features        : fp asimd evtstrm aes pmull sha1 sha2 crc32
CPU implementer : 0x51
CPU architecture: 8
CPU variant     : 0xa
CPU part        : 0x801
CPU revision    : 4

processor       : 4
BogoMIPS        : 38.40
Features        : fp asimd evtstrm aes pmull sha1 sha2 crc32
CPU implementer : 0x51
CPU architecture: 8
CPU variant     : 0xa
CPU part        : 0x800
CPU revision    : 1

processor       : 5
BogoMIPS        : 38.40
Features        : fp asimd evtstrm aes pmull sha1 sha2 crc32
CPU implementer : 0x51
CPU architecture: 8
CPU variant     : 0xa
CPU part        : 0x800
CPU revision    : 1

processor       : 6
BogoMIPS        : 38.40
Features        : fp asimd evtstrm aes pmull sha1 sha2 crc32
CPU implementer : 0x51
CPU architecture: 8
CPU variant     : 0xa
CPU part        : 0x800
CPU revision    : 1

processor       : 7
BogoMIPS        : 38.40
Features        : fp asimd evtstrm aes pmull sha1 sha2 crc32
CPU implementer : 0x51
CPU architecture: 8
CPU variant     : 0xa
CPU part        : 0x800
CPU revision    : 1

Hardware        : Qualcomm Technologies, Inc MSM8998
";



    }
}
