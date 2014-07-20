using System;
using System.Collections.Generic;
using System.IO;
using TShockAPI;
using Newtonsoft.Json;

namespace PingPlugin
{
	public class PingConfig
	{
		public string CommandName = "ping";

		public static string ConfigPath
		{
			get
			{
				return Path.Combine(TShock.SavePath, "ping.json");
			}
		}

		public static PingConfig Read()
		{
			if (!File.Exists(ConfigPath))
				File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new PingConfig(), Formatting.Indented));

			return JsonConvert.DeserializeObject<PingConfig>(File.ReadAllText(ConfigPath));
		}

		public void Write()
		{
			File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
		}
	}
}
