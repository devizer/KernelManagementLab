using System;
using System.Linq;

namespace ReactGraphLab
{
    class StartupOptions
    {
        public static bool NeedRessponseCompression => GetBooleanEnvVar("FORCE_HTTPS_REDIRECT");

        public static bool NeedHttpRedirect => GetBooleanEnvVar("FORCE_HTTPS_REDIRECT");

        private static bool GetBooleanEnvVar(string varName)
        {
            var raw = Environment.GetEnvironmentVariable(varName);
            string[] yes = new[] {"On", "True", "1"};
            return yes.Any(x => x.Equals(raw, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}