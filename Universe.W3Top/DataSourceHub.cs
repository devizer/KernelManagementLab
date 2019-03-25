using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ReactGraphLab
{
    public class DataSourceHub : Hub
    {
        public async Task ReceiveDataSource(object dataSource)
        {
            await Clients.All.SendAsync("ReceiveDataSource", dataSource);
        }
    }
}