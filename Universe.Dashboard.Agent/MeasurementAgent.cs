using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using KernelManagementJam.DebugUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.Agent
{
    public class MeasurementAgent : IHostedService, IDisposable
    {
        public IServiceProvider Services { get; }
        private readonly ILogger<MeasurementAgent> _logger;
        private Timer _timer;
        
       public MeasurementAgent(ILogger<MeasurementAgent> logger, IServiceProvider services)
        {
            _logger = logger;
            Services = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(new TimerCallback(Tick), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            return Task.CompletedTask;
        }
        
        private void Tick(object state)
        {
            // Console.WriteLine("MeasurementAgent::Tick()");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            // PreciseTimer.Shutdown.Set();
            FlushHistory();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        void FlushHistory()
        {
            Stopwatch sw = Stopwatch.StartNew();
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DashboardContext>();
                HistoryLogic history = new HistoryLogic(db);
                // TODO: It is NOT thread safe. Need to implement clone?
                history.Save("NetStatDataSource.By_1_Seconds", NetStatDataSource.Instance.By_1_Seconds);
                history.Save("NetStatDataSource", NetStatDataSource.Instance);

                history.Save("BlockDiskDataSource.By_1_Seconds", BlockDiskDataSource.Instance.By_1_Seconds);
                history.Save("BlockDiskDataSource", BlockDiskDataSource.Instance);

                history.Save("MemorySummaryDataSource.By_1_Seconds", MemorySummaryDataSource.Instance.By_1_Seconds);
                history.Save("MemorySummaryDataSource", MemorySummaryDataSource.Instance);
                
                double msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
                Console.WriteLine($"History flushed in {msec:n1} milliseconds");
            }
        }
    }
}