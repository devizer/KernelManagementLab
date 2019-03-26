using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
            PreJit();
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

        void PreJit()
        {
            // It is better to pre-jit synchronously at Startup.ConfigureServices
            return;
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DashboardContext>();
                HistoryLogic logic = new HistoryLogic(db);
                logic.Save("StartAt", new{At = DateTime.UtcNow});
            }
        }

        void FlushHistory()
        {
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DashboardContext>();
                Stopwatch sw = Stopwatch.StartNew();
                HistoryLogic history = new HistoryLogic(db);
                // It is NOT thread safe
                history.Save("NetStatDataSource.By_1_Seconds", NetStatDataSource.Instance.By_1_Seconds);
                history.Save("NetStatDataSource", NetStatDataSource.Instance);
                double msec = sw.ElapsedTicks * 1000d / Stopwatch.Frequency;
                Console.WriteLine($"History flushed in {msec:n1} milliseconds");
            }
        }
    }
}