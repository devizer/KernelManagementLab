using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Tests
{
    public class TestConsole
    {
        static bool Done = false;
        public static void Setup()
        {
            if (!Done)
            {
                Done = true;
                Console.SetOut(new TW());
            }
            
        }

        class TW : TextWriter
        {
            public override Encoding Encoding { get; }

            public override void WriteLine(string value)
            {
                TestContext.Progress.WriteLine(value);
            }

            public override void Write(string format, params object[] arg)
            {
                this.WriteLine(string.Format(format, arg));
            }
        }
    }
}