using System;

namespace Universe.FioStream.Binaries
{
    public class OrderedLinuxCandidates
    {

        public class LinuxCandidate
        {
            public string Arch { get; set; }
            public Version FioVersion { get; set; }
            public string Codename { get; set; }
            public bool HasLibAio { get; set; }
            public string Url { get; set; }
            public Version LibCVersion { get; set; } 
        }

        class Codename
        {
            public string Name { get; set; }
            public Version LibCVersion { get; set; }

            public Codename(string name, Version libCVersion)
            {
                Name = name;
                LibCVersion = libCVersion;
            }

            public override string ToString()
            {
                return $"{nameof(Name)}: {Name}, {nameof(LibCVersion)}: {LibCVersion}";
            }
        }

        private static readonly Codename[] Codenames = new[]
        {
            new Codename("hirsute", new Version("2.33")),
            new Codename("groovy", new Version("2.32")),
            new Codename("focal", new Version("2.31")),
            new Codename("bullseye", new Version("2.31")),
            new Codename("buster", new Version("2.28")),
            new Codename("bionic", new Version("2.27")),
            new Codename("stretch", new Version("2.24")),
            new Codename("jessie", new Version("2.19")),
            new Codename("trusty", new Version("2.19")),
            new Codename("precise", new Version("2.15")),
            new Codename("wheezy", new Version("2.13")),
            
            new Codename("centosstream", new Version("2.28")),
            new Codename("rhel8", new Version("2.28")),
            new Codename("rhel7", new Version("2.17")),
            new Codename("rhel6", new Version("2.12")),
        };
        
        private const string RawList = @"
fio-2.21-amd64-bionic.gz
fio-2.21-amd64-jessie.gz
fio-2.21-amd64-precise.gz
fio-2.21-amd64-rhel6.gz
fio-2.21-amd64-rhel7.gz
fio-2.21-amd64-stretch.gz
fio-2.21-amd64-trusty.gz
fio-2.21-amd64-wheezy.gz
fio-2.21-amd64-xenial.gz
fio-2.21-arm64-bionic.gz
fio-2.21-arm64-trusty.gz
fio-2.21-arm64-xenial.gz
fio-2.21-armel-stretch.gz
fio-2.21-armel-wheezy.gz
fio-2.21-armhf-bionic.gz
fio-2.21-armhf-precise.gz
fio-2.21-armhf-xenial.gz
fio-2.21-i386-bionic.gz
fio-2.21-i386-precise.gz
fio-2.21-i386-xenial.gz
fio-2.21-libaio-amd64-bionic.gz
fio-2.21-libaio-amd64-jessie.gz
fio-2.21-libaio-amd64-precise.gz
fio-2.21-libaio-amd64-rhel6.gz
fio-2.21-libaio-amd64-rhel7.gz
fio-2.21-libaio-amd64-stretch.gz
fio-2.21-libaio-amd64-trusty.gz
fio-2.21-libaio-amd64-wheezy.gz
fio-2.21-libaio-amd64-xenial.gz
fio-2.21-libaio-arm64-bionic.gz
fio-2.21-libaio-arm64-trusty.gz
fio-2.21-libaio-arm64-xenial.gz
fio-2.21-libaio-armel-stretch.gz
fio-2.21-libaio-armel-wheezy.gz
fio-2.21-libaio-armhf-bionic.gz
fio-2.21-libaio-armhf-precise.gz
fio-2.21-libaio-armhf-xenial.gz
fio-2.21-libaio-i386-bionic.gz
fio-2.21-libaio-i386-precise.gz
fio-2.21-libaio-i386-xenial.gz
fio-2.21-libaio-mips64el-stretch.gz
fio-2.21-libaio-powerpc-wheezy.gz
fio-2.21-libaio-ppc64el-bionic.gz
fio-2.21-libaio-ppc64el-trusty.gz
fio-2.21-libaio-ppc64el-xenial.gz
fio-2.21-mips64el-stretch.gz
fio-2.21-powerpc-wheezy.gz
fio-2.21-ppc64el-bionic.gz
fio-2.21-ppc64el-trusty.gz
fio-2.21-ppc64el-xenial.gz
fio-3.16-amd64-bionic.gz
fio-3.16-amd64-buster.gz
fio-3.16-amd64-centosstream.gz
fio-3.16-amd64-focal.gz
fio-3.16-amd64-jessie.gz
fio-3.16-amd64-rhel6.gz
fio-3.16-amd64-rhel7.gz
fio-3.16-amd64-rhel8.gz
fio-3.16-amd64-stretch.gz
fio-3.16-amd64-trusty.gz
fio-3.16-amd64-wheezy.gz
fio-3.16-amd64-xenial.gz
fio-3.16-arm64-bionic.gz
fio-3.16-arm64-focal.gz
fio-3.16-arm64-trusty.gz
fio-3.16-arm64-xenial.gz
fio-3.16-armel-buster.gz
fio-3.16-armel-stretch.gz
fio-3.16-armel-wheezy.gz
fio-3.16-armhf-bionic.gz
fio-3.16-armhf-focal.gz
fio-3.16-armhf-precise.gz
fio-3.16-armhf-xenial.gz
fio-3.16-i386-bionic.gz
fio-3.16-i386-precise.gz
fio-3.16-i386-xenial.gz
fio-3.16-libaio-amd64-bionic.gz
fio-3.16-libaio-amd64-buster.gz
fio-3.16-libaio-amd64-centosstream.gz
fio-3.16-libaio-amd64-focal.gz
fio-3.16-libaio-amd64-jessie.gz
fio-3.16-libaio-amd64-rhel6.gz
fio-3.16-libaio-amd64-rhel7.gz
fio-3.16-libaio-amd64-rhel8.gz
fio-3.16-libaio-amd64-stretch.gz
fio-3.16-libaio-amd64-trusty.gz
fio-3.16-libaio-amd64-wheezy.gz
fio-3.16-libaio-amd64-xenial.gz
fio-3.16-libaio-arm64-bionic.gz
fio-3.16-libaio-arm64-focal.gz
fio-3.16-libaio-arm64-trusty.gz
fio-3.16-libaio-arm64-xenial.gz
fio-3.16-libaio-armel-buster.gz
fio-3.16-libaio-armel-stretch.gz
fio-3.16-libaio-armel-wheezy.gz
fio-3.16-libaio-armhf-bionic.gz
fio-3.16-libaio-armhf-focal.gz
fio-3.16-libaio-armhf-precise.gz
fio-3.16-libaio-armhf-xenial.gz
fio-3.16-libaio-i386-bionic.gz
fio-3.16-libaio-i386-precise.gz
fio-3.16-libaio-i386-xenial.gz
fio-3.16-libaio-mips64el-stretch.gz
fio-3.16-libaio-powerpc-wheezy.gz
fio-3.16-libaio-ppc64el-bionic.gz
fio-3.16-libaio-ppc64el-focal.gz
fio-3.16-libaio-ppc64el-trusty.gz
fio-3.16-libaio-ppc64el-xenial.gz
fio-3.16-mips64el-stretch.gz
fio-3.16-powerpc-wheezy.gz
fio-3.16-ppc64el-bionic.gz
fio-3.16-ppc64el-focal.gz
fio-3.16-ppc64el-trusty.gz
fio-3.16-ppc64el-xenial.gz
fio-3.26-amd64-bionic.gz
fio-3.26-amd64-bullseye.gz
fio-3.26-amd64-buster.gz
fio-3.26-amd64-centosstream.gz
fio-3.26-amd64-focal.gz
fio-3.26-amd64-groovy.gz
fio-3.26-amd64-hirsute.gz
fio-3.26-amd64-rhel8.gz
fio-3.26-amd64-stretch.gz
fio-3.26-amd64-xenial.gz
fio-3.26-arm64-bionic.gz
fio-3.26-arm64-focal.gz
fio-3.26-arm64-xenial.gz
fio-3.26-armel-buster.gz
fio-3.26-armel-stretch.gz
fio-3.26-armhf-bionic.gz
fio-3.26-armhf-focal.gz
fio-3.26-armhf-xenial.gz
fio-3.26-i386-bionic.gz
fio-3.26-i386-xenial.gz
fio-3.26-libaio-amd64-bionic.gz
fio-3.26-libaio-amd64-bullseye.gz
fio-3.26-libaio-amd64-buster.gz
fio-3.26-libaio-amd64-centosstream.gz
fio-3.26-libaio-amd64-focal.gz
fio-3.26-libaio-amd64-groovy.gz
fio-3.26-libaio-amd64-hirsute.gz
fio-3.26-libaio-amd64-rhel8.gz
fio-3.26-libaio-amd64-stretch.gz
fio-3.26-libaio-amd64-xenial.gz
fio-3.26-libaio-arm64-bionic.gz
fio-3.26-libaio-arm64-focal.gz
fio-3.26-libaio-arm64-xenial.gz
fio-3.26-libaio-armel-buster.gz
fio-3.26-libaio-armel-stretch.gz
fio-3.26-libaio-armhf-bionic.gz
fio-3.26-libaio-armhf-focal.gz
fio-3.26-libaio-armhf-xenial.gz
fio-3.26-libaio-i386-bionic.gz
fio-3.26-libaio-i386-xenial.gz
fio-3.26-libaio-mips64el-stretch.gz
fio-3.26-libaio-ppc64el-bionic.gz
fio-3.26-libaio-ppc64el-focal.gz
fio-3.26-libaio-ppc64el-xenial.gz
fio-3.26-mips64el-stretch.gz
fio-3.26-ppc64el-bionic.gz
fio-3.26-ppc64el-focal.gz
fio-3.26-ppc64el-xenial.gz
";
    }
}