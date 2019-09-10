using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KernelManagementJam;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Timeout;
using Universe.Dashboard.Agent;
using Universe.Dashboard.DAL;

namespace Universe.W3Top
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
            CreateOrUpgradeDb();

            services.AddDbContext<DashboardContext>(options =>
            {
                DashboardContextOptionsFactory.ApplyOptions(options);
            });

            services.AddSingleton<DiskBenchmarkQueue>(new DiskBenchmarkQueue(() => new DashboardContext()));
            services.AddScoped<DiskBenchmarkDataAccess>();
            
            services
                .AddMvc(options => { options.Filters.Add(new KillerActionFilter()); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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
            app.UseMiddleware<PreventSpaHtmlCachingMiddleware>();
            if (!env.IsProduction())
                app.UseMiddleware<KillerMiddleware>();
            
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
            app.UseSignalR(routes =>
            {
                routes.MapHub<DataSourceHub>("/dataSourceHub");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
                
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

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var urlBase = "http://localhost:5050";
                if (IpConfig.Addresses.Any()) urlBase = IpConfig.Addresses.First();
                Uri uri = new Uri(urlBase);
                var server = uri.Host;
                var port = uri.Port;
                WaitForTcp.Run(server, port, 30);
                this.PreJitAspNet();
            });

        }
    }
}