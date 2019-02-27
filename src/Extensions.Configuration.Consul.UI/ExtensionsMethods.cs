using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Extensions.Configuration.Consul.UI
{
    public static class ExtensionsMethods
    {
        public static IServiceCollection AddConsulConfigurationCenter(this IServiceCollection services, UIOptions options, int blockingQueryWaitSeconds = 180)
        {
            services.AddConsulConfigurationCenter(blockingQueryWaitSeconds: blockingQueryWaitSeconds);
            services.AddSingleton(options);
            services.AddSingleton<IHostedService, UIHostedService>();
            return services;
        }

        public static IServiceCollection AddConsulConfigurationCenter(this IServiceCollection services, string ip, int port, int blockingQueryWaitSeconds = 180)
        {
            services.AddConsulConfigurationCenter(new UIOptions(ip, port), blockingQueryWaitSeconds);
            return services;
        }
    }
}
