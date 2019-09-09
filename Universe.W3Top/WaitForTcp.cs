using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using KernelManagementJam;

namespace Universe.W3Top
{
    class WaitForTcp
    {
        public static bool Run(string server, int port, int timeout)
        {
            Console.WriteLine($"Waiting for {server}:{port}");
            Stopwatch sw = Stopwatch.StartNew();
            Exception error = null;
            do
            {
                try
                {
                    TcpClient client = new TcpClient();
                    using (client)
                    {
                        client.Connect(server, port);
                        Console.WriteLine(
                            $"TCP connection to {server}:{port} established in {sw.ElapsedMilliseconds:n0} milliseconds");
                        return true;
                    }
                }
                catch(Exception ex)
                {
                    error = ex;
                    Thread.Sleep(42);
                }

            } while (sw.ElapsedMilliseconds <= timeout * 1000);
            
            Console.WriteLine($"Warning! TCP connection to {server}:{port} is NOT available during {sw.ElapsedMilliseconds:n0} milliseconds. {error.GetExceptionDigest()}");
            return false;

        }
    }
}