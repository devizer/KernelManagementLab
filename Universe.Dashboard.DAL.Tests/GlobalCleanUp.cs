using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using KernelManagementJam;
using NUnit.Framework;

[SetUpFixture]
public class GlobalCleanUp
{

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
        foreach (var a in OnShutdown) a();
        // Parallel.ForEach(OnShutdown, x => x());
    }
    
    private static List<Action> OnShutdown = new List<Action>();
    private static readonly object Sync = new object();
    private static int Counter = 0;

    public static void Enqueue(string title, Action action)
    {
        lock (Sync)
        {
            OnShutdown.Add(() =>
            {
                int counter;
                Stopwatch sw = Stopwatch.StartNew();
                try
                {
                    action();
                    counter = Interlocked.Increment(ref Counter);
                    Console.WriteLine($"[Cleanup] {("#"+counter),3} OK: {title} in {sw.ElapsedMilliseconds:n0} msec");
                }
                catch (Exception ex)
                {
                    counter = Interlocked.Increment(ref Counter);
                    Console.WriteLine(
                        $"[Cleanup] {("#"+counter),3} FAIL: {title} in {sw.ElapsedMilliseconds:n0} msec. {ex.GetExceptionDigest()}");
                }
            });
        }
    }
}

