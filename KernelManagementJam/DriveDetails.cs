using System;

namespace KernelManagementJam
{
    public class DriveDetails
    {
        private const StringComparison CMP = System.StringComparison.CurrentCultureIgnoreCase;
        public MountEntry MountEntry { get; set; }
        public bool IsReady { get; set; }
        public long FreeSpace { get; set; }
        public long TotalSize { get; set; }
        public string Format { get; set; }

        public bool IsTmpFs
        {
            get
            {
                return (MountEntry?.FileSystem ?? "").EndsWith("tmpfs", CMP);
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
                var device = (MountEntry?.Device ?? "");
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
                try
                {
                    Uri u = new Uri(MountEntry?.Device ?? "");
                    return !string.IsNullOrEmpty(u.Host) && 
                           ("http".Equals(u.Scheme, CMP) || "https".Equals(u.Scheme, CMP));
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}