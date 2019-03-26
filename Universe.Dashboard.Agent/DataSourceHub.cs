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
            // PreciseTimer.AllTheTimerFinished += this.FlushDataSource;
        }

        private void FlushDataSource()
        {
            Console.WriteLine("DataSourceHub --> Flushing data source to clients");
            var dataSource = NetStatDataSource.Instance.By_1_Seconds;
            ReceiveDataSource(dataSource).Wait();
        }

        public async Task ReceiveDataSource(object dataSource)
        {
            await Clients.All.SendAsync("ReceiveDataSource", dataSource);
        }

        protected override void Dispose(bool disposing)
        {
            Console.WriteLine($"DataSourceHub::DISPOSE(disposing={disposing})");
        }
    }
}
