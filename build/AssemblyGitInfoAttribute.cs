using System;
using System.Linq;
using System.Reflection;

namespace Universe
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyGitInfoAttribute : Attribute
    {
        private static readonly DateTime ZeroDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public string Branch { get; }
        public int Counter { get; }
        public DateTime DateTimeUtc { get; }

        public AssemblyGitInfoAttribute(string branch, int counter, long secondsSince1970)
        {
            Branch = branch;
            Counter = counter;
            DateTimeUtc = ZeroDate.AddSeconds(secondsSince1970);
        }

        public static AssemblyGitInfoAttribute GetGitInfo(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            object[] arr = assembly.GetCustomAttributes(typeof(AssemblyGitInfoAttribute), false);
            AssemblyGitInfoAttribute attr = (AssemblyGitInfoAttribute) arr.FirstOrDefault();
            return attr;
        }

        public override string ToString()
        {
            return $"{nameof(Branch)}: {Branch}, {nameof(Counter)}: {Counter}, {nameof(DateTimeUtc)}: {DateTimeUtc.ToString("R")}";
        }
    }
}
