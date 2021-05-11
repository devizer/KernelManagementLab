using System;

namespace Universe.FioStream.Tests
{
    public class PicoLogger : IPicoLogger
    {
        public static PicoLogger Instance = new PicoLogger();
        public void LogInfo(string message) => Console.WriteLine(message);
        public void LogWarning(string message) => Console.WriteLine($"WARNING! {message}");
    }
}