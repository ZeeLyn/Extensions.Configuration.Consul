using System;
using System.IO;
using Consul;
using Extensions.Configuration.Consul;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Example
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
			ConsulConfigurationExtensions.Shutdown();
		}
		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>

			WebHost.CreateDefaultBuilder(args).ConfigureAppConfiguration((context, config) =>
			{
				config.SetBasePath(Directory.GetCurrentDirectory());
				config.AddCommandLine(args);
				config.AddJsonFile("appsettings.json", false, true);
				config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", false, true);
				config.AddConsul(new ConsulClientConfiguration
				{
					Address = new Uri("http://192.168.1.142:8500")
				}, new ConsulQueryOptions
				{
					Prefix = "AppSetting/",
					TrimPrefix = true
				}, true);
			}).UseStartup<Startup>();
	}
}
