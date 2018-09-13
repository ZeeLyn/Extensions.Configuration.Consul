﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Extensions.Configuration.Consul;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Example
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args).ConfigureAppConfiguration((context, config) =>
			{
				config.SetBasePath(Directory.GetCurrentDirectory());
				config.AddJsonFile("appsettings.json", false, true);
				config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", false, true);
				//config.AddConsul("http://192.168.1.142:8500", "", "Db/", "dc1", true);
				//config.AddConsul("http://192.168.1.158:8500", "", "Db/", "dc1", true);
				config.AddConsul(new List<ConsulAgentConfiguration>
				{
					new ConsulAgentConfiguration
					{
						ClientConfiguration = new Consul.ConsulClientConfiguration
						{
							Address = new Uri("http://192.168.1.142:8500"),
							Datacenter = "dc1"
						},
						QueryOptions = new ConsulQueryOptions
						{
							Prefix = "Db/",
							TrimPrefix = true
						}
					},
					new ConsulAgentConfiguration
					{
						ClientConfiguration = new Consul.ConsulClientConfiguration
						{
							Address = new Uri("http://192.168.1.158:8500"),
							Datacenter = "dc1"
						},
						QueryOptions = new ConsulQueryOptions
						{
							Prefix = "Db/",
							TrimPrefix = true
						}
					}
				}, true);
			}).UseStartup<Startup>();
	}
}