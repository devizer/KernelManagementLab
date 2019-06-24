using System;

namespace Tests
{
    public static class LambdaGC
    {
        public static Action OnShutdown = delegate { };

        private static Impl trigger = new Impl();
        
        class Impl
        {
            ~Impl()
            {
                Console.WriteLine("Global Resource Collector starting ...");
                int okCount = 0;
                Delegate[] invocationList = LambdaGC.OnShutdown.GetInvocationList();
                foreach (var d in invocationList)
                {
                    try
                    {
                        d.DynamicInvoke();
                        okCount += 1;
                    }
                    catch
                    {
                    }
                }
                
                Console.WriteLine($"Global Resource Collector Completed: {okCount-1} of {invocationList.Length-1} are ok");
            }
        }

    }
}