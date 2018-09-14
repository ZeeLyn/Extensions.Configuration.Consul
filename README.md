# Extensions.Configuration.Consul

## 1.Register

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
				config.SetBasePath(Directory.GetCurrentDirectory());
				config.AddJsonFile("appsettings.json", false, true);
				config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", false, true);
				config.AddConsul("http://192.168.1.142:8500", "", "Db/", "dc1", true);
			}).UseStartup<Startup>();
	  }
```


```csharp
    public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.AddOptions();
			services.Configure<Configs>(Configuration.GetSection("Db"));
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			var builder = new ContainerBuilder();
			builder.Populate(services);
			builder.RegisterType<LibClass>().InstancePerLifetimeScope();
			builder.RegisterType<SingleClass>().SingleInstance();
			ApplicationContainer = builder.Build();
			return new AutofacServiceProvider(ApplicationContainer);
		}
```

## 2.How to use
### InstancePerLifetimeScope
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

### SingleInstance
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



