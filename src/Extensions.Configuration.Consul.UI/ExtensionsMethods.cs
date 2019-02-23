using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Extensions.Configuration.Consul.UI
{
    public static class ExtensionsMethods
    {

        public static IServiceCollection AddConsulConfigurationCenter(this IServiceCollection services, UIOptions options, int blockingQueryWaitSeconds = 180)
        {
            //services.AddConsulConfigurationCenter(blockingQueryWaitSeconds: blockingQueryWaitSeconds);
            services.AddSingleton<IHostedService, UIHostedService>();
            return services;
        }


    }
}
