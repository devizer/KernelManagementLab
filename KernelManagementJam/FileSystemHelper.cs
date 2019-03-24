using System.IO;
using Mono.Unix;

namespace KernelManagementJam
{
    // May be slow?
    public class FileSystemHelper
    {
        public static string Resolve(string path)
        {
            bool isFile = File.Exists(path);
            bool isDirectory = Directory.Exists(path);
            if (isFile || isDirectory)
            {
                UnixFileSystemInfo info;
                if (UnixFileSystemInfo.TryGetFileSystemEntry(path, out info))
                {
                    if (info.IsSymbolicLink)
                    {
                        UnixSymbolicLinkInfo sym = new UnixSymbolicLinkInfo(path);
                        var linkTo = sym.ContentsPath;
                        string resolved;
                        if (isFile)
                            resolved = Path.Combine(Path.GetDirectoryName(path), linkTo);
                        else
                            resolved = Path.Combine(path, linkTo);

                        string ret;
                        if (isFile)
                            ret = new FileInfo(resolved).FullName;
                        else
                            ret = new DirectoryInfo(resolved).FullName;

                        return ret;
                    }
                }
            }

            return path;
        }

        public static bool IsBlockDevice(string path)
        {
            bool isFile = File.Exists(path);
            bool isDirectory = Directory.Exists(path);
            if (isFile || isDirectory)
            {
                UnixFileSystemInfo info;
                if (UnixFileSystemInfo.TryGetFileSystemEntry(path, out info))
                {
                    return info.IsBlockDevice;
                }
            }

            return false;
        }

        public static bool Exists(string path)
        {
            return (File.Exists(path) || Directory.Exists(path));
        }

    }
}