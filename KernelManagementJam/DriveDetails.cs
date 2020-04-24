using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace KernelManagementJam
{
    public class DriveDetails
    {
        private const StringComparison CMP = System.StringComparison.InvariantCultureIgnoreCase;
        public MountEntry MountEntry { get; set; }
        // Same as MountEntry.Device, but resolved and only if it is a block device
        public string BlockDeviceResolved { get; set; }
        public bool IsReady { get; set; }
        public long FreeSpace { get; set; }
        public long TotalSize { get; set; }
        public string Format { get; set; }
        
        public bool? IsReadonly { get; set; }

        public bool IsBlockDevice
        {
            get { return !string.IsNullOrEmpty(BlockDeviceResolved); }
        }

        public bool IsSwap
        {
            get
            {
                return (MountEntry?.FileSystem ?? "").StartsWith("swap", CMP);
            }
        }

        public bool IsTmpFs
        {
            get
            {
                return
                    (MountEntry?.FileSystem ?? "").EndsWith("tmpfs", CMP)
                    || (MountEntry?.Device ?? "").StartsWith("/dev/zram", CMP);
            }
        }

        public bool IsNetworkShare
        {
            get
            {
                return IsNfs || IsSsh || IsCifs || IsFtp || IsWebDav;
            }
        }

        public bool IsFtp
        {
            get
            {
                var device = MountEntry?.Device ?? "";
                return device.IndexOf("ftpfs#", CMP) >= 0 
                       || device.IndexOf("ftp://", CMP) >= 0 
                       || device.IndexOf("ftps://", CMP) >= 0;

            }
        }

        public bool IsCifs
        {
            get { return (MountEntry?.FileSystem ?? "").IndexOf("cifs", CMP) >= 0; }
        }

        public bool IsSsh
        {
            get { return (MountEntry?.FileSystem ?? "").IndexOf("sshfs", CMP) >= 0; }
        }

        public bool IsNfs
        {
            get { return (MountEntry?.FileSystem ?? "").StartsWith("nfs", CMP); }
        }

        public bool IsWebDav
        {
            get
            {
                var urlCandidate = MountEntry?.Device ?? "";
                if (string.IsNullOrEmpty(urlCandidate)) return false;
                
                Uri uriResult;
                bool isHttpOrHttps = Uri.TryCreate(urlCandidate, UriKind.Absolute, out uriResult)
                              && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
                              && !string.IsNullOrEmpty(uriResult.Host);

                return isHttpOrHttps;
            }
        }

        [JsonIgnore]
        public bool IsWebDav_Legacy
        {
            get
            {
                var urlCandidate = MountEntry?.Device ?? "";
                if (string.IsNullOrEmpty(urlCandidate)) return false;
                try
                {
                    Uri u = new Uri(urlCandidate);
                    return !string.IsNullOrEmpty(u.Host) && 
                           ("http".Equals(u.Scheme, CMP) || "https".Equals(u.Scheme, CMP));
                }
                catch
                {
                    return false;
                }
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object DebugInfo { get; set; }
    }

    public static class DriveDetailsExtensions
    {
        private static readonly string[] PathsToSkip = new[] {"/run", "/sys", "/dev"};
        private static readonly string[] DevicesToSkip = new[] {"shm", "overlay"};

        public static IEnumerable<DriveDetails> FilterForHuman(this IEnumerable<DriveDetails> list)
        {
            var filtered = list
                .Where(x => x.TotalSize > 0)
                .Where(x => !PathsToSkip.Any(skip =>
                    x.MountEntry.MountPath == $"{skip}" || x.MountEntry.MountPath.StartsWith($"{skip}/")))
                .Where(x => !DevicesToSkip.Contains(x.MountEntry.Device))
                .ToArray();

            // double / (root) entries: ext4 (or another) and rootfs
            var countOfRoot = filtered.Count(x => x.MountEntry?.MountPath == "/");
            var countOfAnotherRoot = filtered.Count(x => x.MountEntry?.MountPath == "/" && x.MountEntry?.FileSystem != "rootfs" && x.MountEntry?.FileSystem?.Length > 0);
            if (countOfRoot > 1 && countOfAnotherRoot > 0)
                filtered = filtered
                    .Where(x => !(x.MountEntry?.MountPath == "/" && x.MountEntry?.FileSystem == "rootfs"))
                    .ToArray();

            return filtered;
        }
    }
}