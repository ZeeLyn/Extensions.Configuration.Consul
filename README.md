# Extensions.Configuration.Consul
Configuration center based on consul.

# Package & Status
Package | NuGet
---------|------
Extensions.Configuration.Consul|[![NuGet package](https://buildstats.info/nuget/Extensions.Configuration.Consul)](https://www.nuget.org/packages/Extensions.Configuration.Consul)



# Configuration

## Hardcoded configuration
```csharp
  public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}
		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args).ConfigureAppConfiguration((context, config) =>
			{
				config.AddConsul("http://127.0.0.1:8500");
			}).UseStartup<Startup>();
	  }
```

## Command line configuration
Command | Describetion
---------|------
consul-configuration-addr|Consul agent address
consul-configuration-token|ACL Token HTTP API
consul-configuration-dc|Consul data center
consul-configuration-folder|Prefix of key



```csharp
  public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}
		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args).ConfigureAppConfiguration((context, config) =>
			{
				config.AddConsul(args);
			}).UseStartup<Startup>();
	  }
```


## Reload when modified
```csharp
    public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddConsulConfigurationCenter();

			services.AddOptions();
			services.Configure<Configs>(Configuration.GetSection("TestConfig"));
		
		}
```

# Usage
## InstancePerLifetimeScope
```csharp
  public class LibClass
	{
		private Configs Config;
		public LibClass(IOptionsSnapshot<Configs> config)
		{
			Config = config.Value;
		}

		public Configs Get()
		{
			return Config;
		}
	}
```

## SingleInstance
```csharp
  public class SingleClass
	{
		private Configs Config;

		public SingleClass(IOptionsMonitor<Configs> config)
		{
			Config = config.CurrentValue;
			config.OnChange(conf =>
			{
				Config = conf;
			});
		}

		public Configs Get()
		{
			return Config;
		}
	}
```



