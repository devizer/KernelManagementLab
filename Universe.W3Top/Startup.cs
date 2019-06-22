using System;
using System.Collections.Generic;
using KernelManagementJam;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Universe.Dashboard.Agent;
using Universe.Dashboard.DAL;
using EF = Universe.Dashboard.DAL.EF;

namespace ReactGraphLab
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            using (StopwatchLog.ToConsole($"Create/Upgrade DB Structure /{DashboardContextOptionsFactory.Family}/"))
            {
                var dashboardContext = new DashboardContext();
                switch (DashboardContextOptionsFactory.Family)
                {
                    case EF.Family.MySql:
                        // Console.WriteLine($"MySQL CONNECTION STRING: [{DashboardContextOptions4MySQL.ConnectionString}]");
                        using (StopwatchLog.ToConsole($"Check RDBMS health"))
                        {
                            var exception = EFHealth.WaitFor(dashboardContext, 30000);
                            if (exception != null)
                                Console.WriteLine($"RDBMS is not ready. {exception.GetExceptionDigest()}");
                        }
                        
                        EFMigrations.Migrate_MySQL(dashboardContext, DashboardContextOptionsFactory.MigrationsTableName);
                        break;
                    
                    case EF.Family.Sqlite:
                        dashboardContext.Database.Migrate();
                        break;
                    
                    default:
                        throw new ArgumentException($"Unsupported DB provider family {DashboardContextOptionsFactory.Family}");
                }
            }

            services.AddDbContext<DashboardContext>(options =>
            {
                if (DashboardContextOptionsFactory.Family == EF.Family.Sqlite)
                    options.ApplySqliteOptions(SqliteDatabaseOptions.DbFullPath);
                else
                    options.ApplyMySqlOptions(DashboardContextOptions4MySQL.ConnectionString);
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
                spa.Options.StartupTimeout = TimeSpan.FromSeconds(120);

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

        }
    }
}