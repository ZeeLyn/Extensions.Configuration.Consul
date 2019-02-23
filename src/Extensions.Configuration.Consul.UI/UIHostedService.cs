using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Extensions.Configuration.Consul.UI
{
    public class UIHostedService : IHostedService
    {
        private IWebHost WebHost { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            WebHost = new WebHostBuilder().UseUrls("http://*:5342").UseKestrel().UseContentRoot(Directory.GetCurrentDirectory()).UseStartup<Startup>().Build();
            await WebHost.RunAsync(token: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await WebHost.StopAsync(cancellationToken);
        }
    }

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new CompositeFileProvider(new EmbeddedFileProvider(typeof(Startup).GetTypeInfo().Assembly)),
                ServeUnknownFileTypes = true,
                RequestPath = "/consul-configuration"
            });
            app.UseMvc();
        }
    }
}
