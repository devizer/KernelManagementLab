using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Universe.Dashboard.Agent;
using Universe.Dashboard.DAL;

namespace ReactGraphLab
{
    public class Startup
    {
        private static bool NeedRessponseCompression
        {
            get
            {
                var raw = Environment.GetEnvironmentVariable("RESPONSE_COMPRESSION");
                string[] yes = new[] {"On", "True", "1"};
                var needRessponseCompression = yes.Any(x => x.Equals(raw, StringComparison.InvariantCultureIgnoreCase));
                return needRessponseCompression;
            }
        }

        private static bool NeedHttpRedirect
        {
            get
            {
                var raw = Environment.GetEnvironmentVariable("FORCE_HTTPS_REDIRECT");
                string[] yes = new[] {"On", "True", "1"};
                return yes.Any(x => x.Equals(raw, StringComparison.InvariantCultureIgnoreCase));
            }
        }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            if (NeedRessponseCompression) services.AddResponseCompression(x =>
                {
                    x.MimeTypes = CompressedMimeTypes.List;
                });
            
            services.AddHostedService<MeasurementAgent>();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });

            services.AddDbContext<DashboardContext>(options =>
            {
                options.ApplyDashboardDbOptions(DashboardContextDefaultOptions.DbFullPath);
            });
            
            // As same db is used for both design and runtime we pre-cache and pre-jit db access
            // using default ctor
            new DashboardContext().Database.Migrate();
            
            Stopwatch sw = Stopwatch.StartNew();
            Console.WriteLine("Waiting for a first round of /proc/mounts diagnostic: ");
            MountsDataSource.IsFirstIterationReady.WaitOne();
            Console.WriteLine($"First round of /proc/mounts diagnostic is ready, {sw.ElapsedMilliseconds:n0} milliseconds");

            NetStatDataSourcePersistence.PreJit();

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
                if (NeedHttpRedirect) app.UseHsts();
            }

            if (NeedHttpRedirect) app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            
            app.UseCors("CorsPolicy");
            app.UseSignalR(routes =>
            {
                routes.MapHub<DataSourceHub>("/dataSourceHub");
            });

            if (NeedRessponseCompression) app.UseResponseCompression();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                ISpaBuilder spaCopy = spa;
                spa.Options.StartupTimeout = TimeSpan.FromSeconds(120);
                // spa.Options.DefaultPage = "/index.cshtml";
                

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

        }
    }
}