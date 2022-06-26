using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using KernelManagementJam;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Universe.Dashboard.DAL;

namespace Universe.Dashboard.Agent.Profiling
{
    public partial class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<DashboardContext>(options =>
            {
                DashboardContextOptionsFactory.ApplyOptions(options);
            });

            services.AddSingleton<DiskBenchmarkQueue>(new DiskBenchmarkQueue(() => new DashboardContext()));
            services.AddScoped<DiskBenchmarkDataAccess>();
            
            
            services.AddHostedService<MeasurementAgent>();


            
            // As same db is used for both design and runtime we pre-cache and pre-jit db access
            // using default ctor
            
            NewVersionFetcher.Configure();

            using (StopwatchLog.ToConsole("Prepare shared /proc/mounts data source"))
            {
                MountsDataSource.IsFirstIterationReady.WaitOne();
            }

            using (StopwatchLog.ToConsole("Prepare shared /proc/swaps data source"))
            {
                SwapsDataSource.IsFirstIterationReady.WaitOne();
            }


            services.AddCors(options => options.AddPolicy("CorsPolicy",
                builder =>
                {
                    builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowAnyOrigin()
                        // .AllowCredentials()
                        ;
                }));
            
            services.AddSignalR(x =>
            {
                x.EnableDetailedErrors = true;
                x.SupportedProtocols = new List<string>() {"longPolling"};
                // x.HandshakeTimeout = TimeSpan.FromSeconds(2);
            });
            
            
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            app.Use(async (context, next) =>
            {
                Action dumpHeaders = () => DumpHeaders(context);
                dumpHeaders.RunOnly(count: 4, "Dump Request Headers for " + context.Request.Path + context.Request.PathBase);
                await next.Invoke();
            });

            
            lifetime.ApplicationStopping.Register(() =>
            {
                PreciseTimer.Shutdown.Set();
                // NetStatDataSourcePersistence.Flush();
            });
            
            
            

 
        }

        static void DumpHeaders(HttpContext context)
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine($"About {context.Request.Method} {context.Request.GetDisplayUrl()}:");
            int n = 0;
            info.AppendLine($"  - Connection: {context.Connection}");
            info.AppendLine($"  - Connection: {context.Connection?.GetType()}");
            info.AppendLine($"  - Connection.RemoteIpAddress: {context.Connection?.RemoteIpAddress}");
            foreach (KeyValuePair<string,StringValues> header in context.Request.Headers)
            {
                foreach (var value in header.Value)
                {
                    info.AppendLine($"  - {header.Key} #{++n}: '{value}'");
                }
            }

            // info.AppendLine(context.AsJson());
            
            Console.WriteLine(info);
        }
    }
}
