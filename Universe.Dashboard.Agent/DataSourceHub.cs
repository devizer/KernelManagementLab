using System;
using Microsoft.AspNetCore.SignalR;

namespace Universe.Dashboard.Agent
{
    public class DataSourceHub : Hub, IDisposable
    {
        public DataSourceHub()
        {
#if DEBUG
            Console.WriteLine("DataSourceHub::ctor");
#endif
        }

        protected override void Dispose(bool disposing)
        {
#if DEBUG
            Console.WriteLine($"DataSourceHub::DISPOSE(disposing={disposing})");
#endif
        }
    }
}
