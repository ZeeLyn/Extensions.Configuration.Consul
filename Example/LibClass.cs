using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Example
{
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
}
