using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Universe.Dashboard.Agent
{
    public class FakeRootFs
    {
        public static readonly string Root = @"pseudo-root";

        public static string Transform(string realPath)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return Root + realPath.Replace("/", "\\");

            return realPath;
        }
    }
}
