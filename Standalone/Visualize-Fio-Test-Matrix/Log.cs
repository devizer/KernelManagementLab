using System;
using System.Diagnostics;

namespace VisualizeFioTestMatrix
{
    class Log
    {
        public static IDisposable Duration(string title)
        {
            return new Finish(Stopwatch.StartNew(), title);

        }

        class Finish : IDisposable
        {
            private Stopwatch StartAt { get; }
            private string Message { get; }

            public Finish(Stopwatch startAt, string message)
            {
                StartAt = startAt;
                Message = message;
            }

            public void Dispose()
            {
                var seconds = StartAt.ElapsedTicks / (double) Stopwatch.Frequency;
                Console.WriteLine($"Duration is {seconds:n3} secs for {Message}");
            }
        }
    }
}
