using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Example
{
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
}
