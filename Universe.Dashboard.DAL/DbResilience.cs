using System;
using System.Diagnostics;
using KernelManagementJam;
using Polly;

namespace Universe.Dashboard.DAL
{
    
    public class DbResilience
    {
        public static void ExecuteWriting(string operation, Action action, 
            int totalMilliseconds = 3000,
            int retryCount = 42000,
            Func<int, TimeSpan> sleepDurationProvider = null
        )
        {
            if (sleepDurationProvider == null) sleepDurationProvider = (retry) => TimeSpan.FromMilliseconds(1);
            operation = operation ?? "No Name";
            Stopwatch sw = null;
            void OnRetry(Exception exception, TimeSpan timeSpan, int retry, Context context)
            {
                sw = sw ?? Stopwatch.StartNew();
                Console.WriteLine($"Info {sw.ElapsedMilliseconds}. Retry N{retry} at {context.OperationKey}, due to: {exception.GetExceptionDigest()}.");
                if (sw.ElapsedMilliseconds > totalMilliseconds)
                    throw new TimeoutException($"Write to DB Fail. Retry N{retry} of [{context.OperationKey}] at {sw.ElapsedMilliseconds} milliseconds, due to: {exception.GetExceptionDigest()}.", exception);
            }

            Policy waitAndRetry = Policy.Handle<Exception>().WaitAndRetry(retryCount, sleepDurationProvider, OnRetry);
            waitAndRetry.Execute(context => action(), new Context(operation));
        }
        
        public static Policy WritingPolicy
        {
            get
            {
                Stopwatch sw = null;
                void OnRetry(Exception exception, TimeSpan timeSpan, int retry, Context context)
                {
                    sw = sw ?? Stopwatch.StartNew();
                    Console.WriteLine($"Info {sw.ElapsedMilliseconds}. Retry N{retry} at {context.OperationKey}, due to: {exception.GetExceptionDigest()}.");
                    if (sw.ElapsedMilliseconds > 3000)
                        throw new TimeoutException($"Write to DB Fail. Retry N{retry} of [{context.OperationKey}] at {sw.ElapsedMilliseconds} milliseconds, due to: {exception.GetExceptionDigest()}.", exception);
                }

                Policy waitAndRetry = Policy.Handle<Exception>().WaitAndRetry(2000, retry => TimeSpan.FromMilliseconds(1), OnRetry);
                return waitAndRetry;
            }
        }
    }
}