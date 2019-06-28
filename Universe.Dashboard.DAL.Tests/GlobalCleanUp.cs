using System;
using System.Collections.Generic;
using System.Diagnostics;
using KernelManagementJam;
using NUnit.Framework;
using Universe.Dashboard.DAL.Tests;



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
    }
    
    private static bool Subscrined = false; 
    private static List<Action> OnShutdown = new List<Action>();
    private static readonly object Sync = new object();
    public static void Enqueue(string title, Action action)
    {
        lock (Sync)
        {
            if (!Subscrined)
            {
                AppDomain.CurrentDomain.ProcessExit += delegate
                {
                    // foreach (var a in OnShutdown) a();
                };
                Subscrined = true;
            }
        }

        // Console.WriteLine($"Enqueuing {title}");
        OnShutdown.Add(() =>
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                action();
                Console.WriteLine($"[Cleanup] ok: {title} in {sw.ElapsedMilliseconds:n0} msec");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cleanup] FAIL: {title} in {sw.ElapsedMilliseconds:n0} msec. {ex.GetExceptionDigest()}");
            }
        });
    }
}

