using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
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
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("CC9800B9-DFEF-4644-B862-FB630DFB880E"));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                ValidateIssuer = true,
                ValidIssuer = "consul configuration center",

                ValidateAudience = true,
                ValidAudience = "consul configuration center user",

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = validationParameters;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseDefaultFiles();
            app.UseCors(b =>
            {
                b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials();
            });
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
