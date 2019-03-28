using System;
using Microsoft.AspNetCore.SignalR;

namespace ReactGraphLab
{
    public class DataSourceHub : Hub, IDisposable
    {
        public DataSourceHub()
        {
            Console.WriteLine("DataSourceHub::ctor");
        }

        protected override void Dispose(bool disposing)
        {
            Console.WriteLine($"DataSourceHub::DISPOSE(disposing={disposing})");
        }
    }
}
