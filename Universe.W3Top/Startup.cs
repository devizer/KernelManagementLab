using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using KernelManagementJam;
using KernelManagementJam.DebugUtils;


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Serialization;

using Universe.Dashboard.Agent;
using Universe.Dashboard.DAL;
using Universe.FioStream;
using Universe.FioStream.Binaries;

namespace Universe.W3Top
{
    // Migrations 2.x -> 3.x
    // https://docs.microsoft.com/ru-ru/aspnet/core/migration/22-to-30?view=aspnetcore-5.0&tabs=visual-studio
    // https://stackoverflow.com/questions/58392039/how-to-set-json-serializer-settings-in-asp-net-core-3/58392090#58392090
    // "@aspnet/signalr": "^1.1.4", --> "@microsoft/signalr", connection.status is string 
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
            CreateOrUpgradeDb();

            services.AddDbContext<DashboardContext>(options =>
            {
                DashboardContextOptionsFactory.ApplyOptions(options);
            });

            services.AddSingleton<IPicoLogger,FioFeaturesLogger>();
            services.AddSingleton<FioEnginesProvider>(serviceProvider =>
            {
                IPicoLogger picoLogger = serviceProvider.GetService<IPicoLogger>();
                return new FioEnginesProvider(new FioFeaturesCache() {Logger = picoLogger}, picoLogger);
            });
            
            services.AddSingleton<DiskBenchmarkQueue>(new DiskBenchmarkQueue(() => new DashboardContext()));
            services.AddScoped<DiskBenchmarkDataAccess>();
            
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                // Use the default property (Pascal) casing
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            if (StartupOptions.NeedResponseCompression)
                services.AddResponseCompression(x => { x.MimeTypes = CompressedMimeTypes.List; });
            
            services.AddHostedService<MeasurementAgent>();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            
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

            DbPreJitter.PreJIT();

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
            
            var signalR = services.AddSignalR(x =>
            {
                x.EnableDetailedErrors = true;
                // x.SupportedProtocols = new List<string>() {"longPolling"};
                // x.HandshakeTimeout = TimeSpan.FromSeconds(2);
            });

            signalR.AddNewtonsoftJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            
            var ver = Assembly.GetEntryAssembly().GetName().Version;
            var miniProfilerReportFile = Path.Combine(DebugDumper.DumpDir, $"Mini-Profiler.Report-{ver}.txt");
            AdvancedMiniProfilerReport.ReportToFile(miniProfilerReportFile);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            lifetime.ApplicationStarted.Register(() =>
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    if (LinuxMemorySummary.TryParse(out var memoryInfo))
                    {
                        // each fio needs 40+ or about ~250 MB of ram
                        var mbAvail = memoryInfo.Available / 1024;
                        int maxDiscoveryThreads = (int) Math.Max(1, mbAvail / 250);
                        FioEnginesProvider.DiscoveryThreadsLimit = maxDiscoveryThreads;
                        Console.WriteLine($"[fio-features] Available memory: {mbAvail:n0} MB, Max Discovery Threads: {maxDiscoveryThreads}");
                    }
                    FioEnginesProvider enginesProvider = scope.ServiceProvider.GetRequiredService<FioEnginesProvider>();
                    Thread t = new Thread(_ => enginesProvider.Discovery()) {IsBackground = true};
                    t.Start();
                }
            });
            
            app.Use(async (context, next) =>
            {
                Action dumpHeaders = () => DumpHeaders(context);
                dumpHeaders.RunOnly(count: 4, "Dump Request Headers for " + context.Request.Path);
                await next.Invoke();
            });

            app.UseMiddleware<PreventSpaHtmlCachingMiddleware>();
            
            if (!env.IsProduction()) app.UseMiddleware<KillerMiddleware>();
            
            lifetime.ApplicationStopping.Register(() =>
            {
                PreciseTimer.Shutdown.Set();
                // NetStatDataSourcePersistence.Flush();
            });
            
            if (env.IsDevelopment() || true)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }

            if (!env.IsDevelopment())
            {
                if (StartupOptions.NeedHttpRedirect) app.UseHsts();
            }

            if (StartupOptions.NeedHttpRedirect) app.UseHttpsRedirection();
            if (StartupOptions.NeedResponseCompression) app.UseResponseCompression();
            
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            
            app.UseCors("CorsPolicy");


            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                
                endpoints.MapHub<DataSourceHub>("/api-hubs/dataSourceHub");
            });
            
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                spa.Options.StartupTimeout = TimeSpan.FromSeconds(180);

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

            var urlBase = IpConfig.Addresses.FirstOrDefault();
            if (urlBase != null)
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Uri uri = new Uri(urlBase);
                    WaitForTcp.Run(uri.Host, uri.Port, 30);
                    this.PreJitAspNet();
                });

        }

        static void DumpHeaders(HttpContext context)
        {
            return;
            StringBuilder info = new StringBuilder();
            info.AppendLine($"About {context.Request.Method} {context.Request.GetDisplayUrl()}:");
            info.AppendLine($"  - Connection.RemoteIpAddress: {context.Connection?.RemoteIpAddress}");
            int n = 0;
            // info.AppendLine($"  - Connection: {context.Connection}");
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