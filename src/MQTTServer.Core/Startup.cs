using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet.AspNetCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;

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
            services.AddSingleton(_interceptors);
            if (_options.Server.UseKestrelServer)
            {
                //this adds a hosted mqtt server to the services
                services.AddHostedMqttServer(MqttServerUtility.BuildOptions(_options.Server, _factory, _interceptors).Build());

                services.AddMqttConnectionHandler();

                //this adds websocket support
                if (_options.Server.UseWebSocket)
                    services.AddMqttWebSocketServerAdapter();
            }

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

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
            }

            if (_options.Server.UseKestrelServer)
            {

                if (_options.Server.UseWebSocket)
                    app.UseMqttEndpoint();

                app.UseMqttServer(s => _interceptors.SetServer(s));
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseMiddleware<EmbeddedFileMiddleware>("/index.html", $"{MqttCore.MqttEmbeddedBase}.index.html");
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new Microsoft.AspNetCore.Http.PathString(),
                FileProvider = new EmbeddedFileProvider(typeof(Startup).Assembly, $"{MqttCore.MqttEmbeddedBase}")
            });
        }

    }
}
