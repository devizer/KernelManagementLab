using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Universe.FioStream.Binaries
{
    public class OrderedLinuxCandidates
    {
        public static List<LinuxCandidate> CurrentLinuxCandidates => _CurrentLinuxCandidates.Value;
        private static Lazy<List<LinuxCandidate>> _CurrentLinuxCandidates = new Lazy<List<LinuxCandidate>>(GetCurrentLinuxCandidates);

        public static List<LinuxCandidate> AllLinuxCandidates => _AllLinuxCandidates.Value;
        private static Lazy<List<LinuxCandidate>> _AllLinuxCandidates = new Lazy<List<LinuxCandidate>>(GetAllLinuxCandidates);

        public class LinuxCandidate
        {
            public string Arch { get; set; }
            public string Name { get; set; }
            public Version FioVersion { get; set; }
            public string Codename { get; set; }
            public bool HasLibAio { get; set; }
            public string Url { get; set; }
            public Version LibCVersion { get; set; }

            public override string ToString()
            {
                return $"{Arch,-8}, {FioVersion,-5} LibAIO={HasLibAio,-5}, {nameof(Codename)}: {(Codename + ":" + LibCVersion),-18}, {nameof(Url)}: {Url}";
            }
        }
        

        private static Regex RegExI386 = new Regex(@"^i[3-7]86$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // For Testing only
        public static IEnumerable<LinuxCandidate> FindCandidateByLinuxMachine(string linuxMachine)
        {
            IEnumerable<LinuxCandidate> FindCandidates(string arch)
            {
                return CurrentLinuxCandidates
                    .Where(x => x.Arch.Equals(arch, StringComparison.OrdinalIgnoreCase));
            }

            var ret = new List<LinuxCandidate>();

            // filter and sort by current libc ver, PosixMachine, PosixLongBit and PosixSystem
            // i386 armel armhf arm64 amd64 powerpc mips64el ppc64el
            var machine = linuxMachine?.ToLower() ?? "";
            if (machine == "x86_64")
            {
                // amd64, i386
                ret.AddRange(FindCandidates("amd64"));
                ret.AddRange(FindCandidates("i386"));
            }
            else if (RegExI386.IsMatch(machine))
            {
                // i386, amd64 
                ret.AddRange(FindCandidates("i386"));
                ret.AddRange(FindCandidates("amd64"));
            }
            else if (machine.StartsWith("armv6") || machine.StartsWith("armv5"))
            {
                // armel
                ret.AddRange(FindCandidates("armel"));
            }
            else if (machine.StartsWith("armv7") || machine.StartsWith("armv8"))
            {
                // armhf, arm64
                ret.AddRange(FindCandidates("armhf"));
                ret.AddRange(FindCandidates("arm64"));
            }
            else if (machine.StartsWith("aarch"))
            {
                // arm64, armhf
                ret.AddRange(FindCandidates("arm64"));
                ret.AddRange(FindCandidates("armhf"));
            }
            else if (machine == "ppc")
            {
                // powerpc
                ret.AddRange(FindCandidates("powerpc"));
            }
            else if (machine == "ppc64le")
            {
                // ppc64el
                ret.AddRange(FindCandidates("ppc64el"));
            }
            else if (machine.StartsWith("mips64"))
            {
                // mips64el
                ret.AddRange(FindCandidates("mips64el"));
            }

            return ret;
        }

        private static List<LinuxCandidate> GetAllLinuxCandidates()
        {
            
            var rawArray = RawList_V2 // RawList_V1
                .Split(new[] {'\r', '\n'})
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .ToArray();

            List<LinuxCandidate> ret = new List<LinuxCandidate>();
            foreach (var rawName in rawArray)
            {
                var name = Path.GetFileNameWithoutExtension(rawName);
                bool hasLibAio = rawName.IndexOf("libaio-", StringComparison.OrdinalIgnoreCase) >= 0;
                // v2
                hasLibAio = rawName.IndexOf("libaio-missing", StringComparison.OrdinalIgnoreCase) < 0; 
                var parts = name.Replace("libaio-", "").Split('-');
                var codename = parts[parts.Length - 1];
                var codeInfo = Codenames.FirstOrDefault(x => x.Name.Equals(codename));
                ret.Add(new LinuxCandidate()
                {
                    Arch = parts[parts.Length - 2],
                    Name = name,
                    Codename = codename,
                    // Url = $"https://master.dl.sourceforge.net/project/fio/{rawName}?viasf=1",
                    Url = $"https://master.dl.sourceforge.net/project/fio/ver2/{rawName}?viasf=1",
                    FioVersion = new Version(parts[1]),
                    HasLibAio = hasLibAio,
                    LibCVersion = codeInfo?.LibCVersion 
                });
            }

            ret = ret
                .OrderByDescending(x => x.LibCVersion)
                .ThenByDescending(x => x.FioVersion)
                .ThenByDescending(x => x.HasLibAio ? 1 : 0)
                .ThenBy(x => x.Url)
                .ToList();

            return ret;
            
        }
        private static List<LinuxCandidate> GetCurrentLinuxCandidates()
        {
            var libCVersion = Candidates.LibCVersion;
            return GetAllLinuxCandidates()
                .Where(x => x.LibCVersion == null || libCVersion == null || libCVersion >= x.LibCVersion)
                .ToList();
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
            new Codename("xenial", new Version("2.23")),
            new Codename("jessie", new Version("2.19")),
            new Codename("trusty", new Version("2.19")),
            new Codename("precise", new Version("2.15")),
            new Codename("wheezy", new Version("2.13")),
            
            new Codename("centosstream", new Version("2.28")),
            new Codename("rhel8", new Version("2.28")),
            new Codename("rhel7", new Version("2.17")),
            new Codename("rhel6", new Version("2.12")),
        };

        private const string RawList_V2 = @"
fio-2.21-libaio-0.3.112-amd64-bionic.gz
fio-2.21-libaio-0.3.112-amd64-jessie.gz
fio-2.21-libaio-0.3.112-amd64-rhel6.gz
fio-2.21-libaio-0.3.112-amd64-rhel7.gz
fio-2.21-libaio-0.3.112-amd64-stretch.gz
fio-2.21-libaio-0.3.112-amd64-trusty.gz
fio-2.21-libaio-0.3.112-amd64-wheezy.gz
fio-2.21-libaio-0.3.112-amd64-xenial.gz
fio-2.21-libaio-0.3.112-arm64-bionic.gz
fio-2.21-libaio-0.3.112-arm64-trusty.gz
fio-2.21-libaio-0.3.112-arm64-xenial.gz
fio-2.21-libaio-0.3.112-armel-stretch.gz
fio-2.21-libaio-0.3.112-armel-wheezy.gz
fio-2.21-libaio-0.3.112-armhf-bionic.gz
fio-2.21-libaio-0.3.112-armhf-xenial.gz
fio-2.21-libaio-0.3.112-i386-bionic.gz
fio-2.21-libaio-0.3.112-i386-xenial.gz
fio-2.21-libaio-0.3.112-mips64el-stretch.gz
fio-2.21-libaio-0.3.112-powerpc-wheezy.gz
fio-2.21-libaio-0.3.112-ppc64el-bionic.gz
fio-2.21-libaio-0.3.112-ppc64el-trusty.gz
fio-2.21-libaio-0.3.112-ppc64el-xenial.gz
fio-2.21-libaio-missing-amd64-bionic.gz
fio-2.21-libaio-missing-amd64-jessie.gz
fio-2.21-libaio-missing-amd64-rhel6.gz
fio-2.21-libaio-missing-amd64-rhel7.gz
fio-2.21-libaio-missing-amd64-stretch.gz
fio-2.21-libaio-missing-amd64-trusty.gz
fio-2.21-libaio-missing-amd64-wheezy.gz
fio-2.21-libaio-missing-amd64-xenial.gz
fio-2.21-libaio-missing-arm64-bionic.gz
fio-2.21-libaio-missing-arm64-trusty.gz
fio-2.21-libaio-missing-arm64-xenial.gz
fio-2.21-libaio-missing-armel-stretch.gz
fio-2.21-libaio-missing-armel-wheezy.gz
fio-2.21-libaio-missing-armhf-bionic.gz
fio-2.21-libaio-missing-armhf-xenial.gz
fio-2.21-libaio-missing-i386-bionic.gz
fio-2.21-libaio-missing-i386-xenial.gz
fio-2.21-libaio-missing-mips64el-stretch.gz
fio-2.21-libaio-missing-powerpc-wheezy.gz
fio-2.21-libaio-missing-ppc64el-bionic.gz
fio-2.21-libaio-missing-ppc64el-trusty.gz
fio-2.21-libaio-missing-ppc64el-xenial.gz
fio-3.16-libaio-0.3.112-amd64-bionic.gz
fio-3.16-libaio-0.3.112-amd64-buster.gz
fio-3.16-libaio-0.3.112-amd64-centosstream.gz
fio-3.16-libaio-0.3.112-amd64-focal.gz
fio-3.16-libaio-0.3.112-amd64-jessie.gz
fio-3.16-libaio-0.3.112-amd64-rhel6.gz
fio-3.16-libaio-0.3.112-amd64-rhel7.gz
fio-3.16-libaio-0.3.112-amd64-rhel8.gz
fio-3.16-libaio-0.3.112-amd64-stretch.gz
fio-3.16-libaio-0.3.112-amd64-trusty.gz
fio-3.16-libaio-0.3.112-amd64-wheezy.gz
fio-3.16-libaio-0.3.112-amd64-xenial.gz
fio-3.16-libaio-0.3.112-arm64-bionic.gz
fio-3.16-libaio-0.3.112-arm64-focal.gz
fio-3.16-libaio-0.3.112-arm64-trusty.gz
fio-3.16-libaio-0.3.112-arm64-xenial.gz
fio-3.16-libaio-0.3.112-armel-buster.gz
fio-3.16-libaio-0.3.112-armel-stretch.gz
fio-3.16-libaio-0.3.112-armel-wheezy.gz
fio-3.16-libaio-0.3.112-armhf-bionic.gz
fio-3.16-libaio-0.3.112-armhf-focal.gz
fio-3.16-libaio-0.3.112-armhf-xenial.gz
fio-3.16-libaio-0.3.112-i386-bionic.gz
fio-3.16-libaio-0.3.112-i386-xenial.gz
fio-3.16-libaio-0.3.112-mips64el-stretch.gz
fio-3.16-libaio-0.3.112-powerpc-wheezy.gz
fio-3.16-libaio-0.3.112-ppc64el-bionic.gz
fio-3.16-libaio-0.3.112-ppc64el-focal.gz
fio-3.16-libaio-0.3.112-ppc64el-trusty.gz
fio-3.16-libaio-0.3.112-ppc64el-xenial.gz
fio-3.16-libaio-missing-amd64-bionic.gz
fio-3.16-libaio-missing-amd64-buster.gz
fio-3.16-libaio-missing-amd64-centosstream.gz
fio-3.16-libaio-missing-amd64-focal.gz
fio-3.16-libaio-missing-amd64-jessie.gz
fio-3.16-libaio-missing-amd64-rhel6.gz
fio-3.16-libaio-missing-amd64-rhel7.gz
fio-3.16-libaio-missing-amd64-rhel8.gz
fio-3.16-libaio-missing-amd64-stretch.gz
fio-3.16-libaio-missing-amd64-trusty.gz
fio-3.16-libaio-missing-amd64-wheezy.gz
fio-3.16-libaio-missing-amd64-xenial.gz
fio-3.16-libaio-missing-arm64-bionic.gz
fio-3.16-libaio-missing-arm64-focal.gz
fio-3.16-libaio-missing-arm64-trusty.gz
fio-3.16-libaio-missing-arm64-xenial.gz
fio-3.16-libaio-missing-armel-buster.gz
fio-3.16-libaio-missing-armel-stretch.gz
fio-3.16-libaio-missing-armel-wheezy.gz
fio-3.16-libaio-missing-armhf-bionic.gz
fio-3.16-libaio-missing-armhf-focal.gz
fio-3.16-libaio-missing-armhf-xenial.gz
fio-3.16-libaio-missing-i386-bionic.gz
fio-3.16-libaio-missing-i386-xenial.gz
fio-3.16-libaio-missing-mips64el-stretch.gz
fio-3.16-libaio-missing-powerpc-wheezy.gz
fio-3.16-libaio-missing-ppc64el-bionic.gz
fio-3.16-libaio-missing-ppc64el-focal.gz
fio-3.16-libaio-missing-ppc64el-trusty.gz
fio-3.16-libaio-missing-ppc64el-xenial.gz
fio-3.26-libaio-0.3.112-amd64-bionic.gz
fio-3.26-libaio-0.3.112-amd64-bullseye.gz
fio-3.26-libaio-0.3.112-amd64-buster.gz
fio-3.26-libaio-0.3.112-amd64-centosstream.gz
fio-3.26-libaio-0.3.112-amd64-focal.gz
fio-3.26-libaio-0.3.112-amd64-groovy.gz
fio-3.26-libaio-0.3.112-amd64-hirsute.gz
fio-3.26-libaio-0.3.112-amd64-rhel8.gz
fio-3.26-libaio-0.3.112-amd64-stretch.gz
fio-3.26-libaio-0.3.112-amd64-xenial.gz
fio-3.26-libaio-0.3.112-arm64-bionic.gz
fio-3.26-libaio-0.3.112-arm64-focal.gz
fio-3.26-libaio-0.3.112-arm64-xenial.gz
fio-3.26-libaio-0.3.112-armel-buster.gz
fio-3.26-libaio-0.3.112-armel-stretch.gz
fio-3.26-libaio-0.3.112-armhf-bionic.gz
fio-3.26-libaio-0.3.112-armhf-focal.gz
fio-3.26-libaio-0.3.112-armhf-xenial.gz
fio-3.26-libaio-0.3.112-i386-bionic.gz
fio-3.26-libaio-0.3.112-i386-xenial.gz
fio-3.26-libaio-0.3.112-mips64el-stretch.gz
fio-3.26-libaio-0.3.112-ppc64el-bionic.gz
fio-3.26-libaio-0.3.112-ppc64el-focal.gz
fio-3.26-libaio-0.3.112-ppc64el-xenial.gz
fio-3.26-libaio-missing-amd64-bionic.gz
fio-3.26-libaio-missing-amd64-bullseye.gz
fio-3.26-libaio-missing-amd64-buster.gz
fio-3.26-libaio-missing-amd64-centosstream.gz
fio-3.26-libaio-missing-amd64-focal.gz
fio-3.26-libaio-missing-amd64-groovy.gz
fio-3.26-libaio-missing-amd64-hirsute.gz
fio-3.26-libaio-missing-amd64-rhel8.gz
fio-3.26-libaio-missing-amd64-stretch.gz
fio-3.26-libaio-missing-amd64-xenial.gz
fio-3.26-libaio-missing-arm64-bionic.gz
fio-3.26-libaio-missing-arm64-focal.gz
fio-3.26-libaio-missing-arm64-xenial.gz
fio-3.26-libaio-missing-armel-buster.gz
fio-3.26-libaio-missing-armel-stretch.gz
fio-3.26-libaio-missing-armhf-bionic.gz
fio-3.26-libaio-missing-armhf-focal.gz
fio-3.26-libaio-missing-armhf-xenial.gz
fio-3.26-libaio-missing-i386-bionic.gz
fio-3.26-libaio-missing-i386-xenial.gz
fio-3.26-libaio-missing-mips64el-stretch.gz
fio-3.26-libaio-missing-ppc64el-bionic.gz
fio-3.26-libaio-missing-ppc64el-focal.gz
fio-3.26-libaio-missing-ppc64el-xenial.gz
";
        
        private const string RawList_V1 = @"
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