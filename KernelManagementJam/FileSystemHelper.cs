using System;
using System.IO;
using Mono.Unix;

namespace KernelManagementJam
{
    // May be slow?
    public class FileSystemHelper
    {
        public static string Resolve(string path)
        {
            bool isDirectory = Directory.Exists(path);
            bool isFile = isDirectory ? false : File.Exists(path);
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
            bool isDirectory = Directory.Exists(path);
            bool isFile = isDirectory ? false : File.Exists(path);
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

        public static bool IsBlockDevice_Slow(string path)
        {
            try
            {
                UnixFileSystemInfo info;
                if (UnixFileSystemInfo.TryGetFileSystemEntry(path, out info))
                {
                    return info.IsBlockDevice;
                }

                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            // WTH: Why is not a FileNotFoundException?
            catch (InvalidOperationException ex)
            {
                // Works for Mono.Posix 1.0
                if (ex.Message?.IndexOf("Path", StringComparison.InvariantCultureIgnoreCase) >= 0
                    && ex.Message?.IndexOf("exist", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return false;

                // throw;
                return false;
            }
            catch (Exception)
            {
                // throw;
                return false;
            }
        }

        public static bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        [Obsolete("TODO: /sys/block/.../ro (0|1) should be used, FreeSpace == 0 can be used", true)]
        public static bool IsReadonly(string device)
        {
            throw new NotImplementedException("NO WAY!");
        }

        public static bool IsSymLink(string fullName)
        {
            try
            {
                UnixFileSystemInfo info;
                if (UnixFileSystemInfo.TryGetFileSystemEntry(fullName, out info))
                {
                    return info.FileType == FileTypes.SymbolicLink;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool IsRegularFile(string fullName)
        {
            try
            {
                UnixFileSystemInfo info;
                if (UnixFileSystemInfo.TryGetFileSystemEntry(fullName, out info))
                {
                    return info.FileType == FileTypes.SymbolicLink;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}