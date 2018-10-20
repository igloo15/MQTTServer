using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Timers;
using System.Linq;
using MQTTnet.AspNetCore;
using Microsoft.Extensions.Logging;

namespace MQTTServer.Core
{
    internal class Startup
    {
        private MqttSettings _options;
        private ILoggerFactory _factory;
        private Interceptors _interceptors;
        private MqttModel _model;

        public Startup(IConfiguration configuration, MqttSettings options, ILoggerFactory factory, Interceptors interceptor, MqttModel model)
        {
            Configuration = configuration;
            _options = options;
            _factory = factory;
            _interceptors = interceptor;
            _model = model;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_model);
            //this adds a hosted mqtt server to the services
            services.AddHostedMqttServer(MqttServerUtility.BuildOptions(_options.Server, _factory, _interceptors).Build());

            //this adds tcp server support based on System.Net.Socket
            services.AddMqttTcpServerAdapter();

            //this adds websocket support
            services.AddMqttWebSocketServerAdapter();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        private Timer _timer = new Timer(2000);

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                //if (env.IsDevelopment())
                //{
                //    spa.UseAngularCliServer(npmScript: "start");
                //}
            });

            //app.UseMqttEndpoint();
        }

    }
}
