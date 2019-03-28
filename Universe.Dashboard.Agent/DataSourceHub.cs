using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Universe.Dashboard.Agent;

namespace ReactGraphLab
{
    public class DataSourceHub : Hub, IDisposable
    {
        public DataSourceHub()
        {
            Console.WriteLine("DataSourceHub::ctor");
        }


//        public async Task ReceiveDataSource(object dataSource)
//        {
//            Console.WriteLine("DataSourceHub::ReceiveDataSource()");
//            await Clients.All.SendAsync("ReceiveDataSource", dataSource);
//        }

        protected override void Dispose(bool disposing)
        {
            Console.WriteLine($"DataSourceHub::DISPOSE(disposing={disposing})");
        }
    }
}
