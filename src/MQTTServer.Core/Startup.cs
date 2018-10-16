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

namespace MQTTServer.Core
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

            services.AddSingleton<MqttModel>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        private Timer _timer = new Timer(2000);
        private MqttModel _model;

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

            _model = app.ApplicationServices.GetService<MqttModel>();

            _model
                .AddService("TestService1", "localhost")
                .AddService("TestService2", "127.0.0.1")
                .AddService("TestService3", "192.168.1.1");

            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        int index = 0;
        Random rand = new Random();

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(rand.Next(3) % 3 == 0)
                _model.AddMessage("bill/test", $"TestService{rand.Next(3)}");

            if (rand.Next(3) % 3 == 1)
                _model.AddMessage("home/wonder/test", $"TestService{rand.Next(3)}");

            if (rand.Next(3) % 3 == 2)
                _model.AddMessage("other/home", $"TestService{rand.Next(3)}");

            if(rand.Next(30) % 30 == 15)
            {
                _model.AddService($"MyService{rand.Next(100)}", $"{rand.Next(225)+1}.{rand.Next(225)+1}.{rand.Next(225)+1}.{rand.Next(225)+1}");
            }

            if(rand.Next(100) % 50 == 15)
            {
                _model.GetServices().Skip(rand.Next(_model.GetServices().Count())).FirstOrDefault()?.AddSubscription($"mysub{rand.Next(3)}/test");
            }
                
        }
    }
}
