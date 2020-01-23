using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;

namespace PlayerStatsBot
{
	public class Program
	{
		private readonly Bot bot;
		private const string kCfgFile = "PlayerStatsBot.json";
		private Config config;

		public Config Config => config ?? (config = GetConfig());

		public static void Main()
		{
			Program program = new Program();
		}

		private Program() => bot = new Bot(this);

		public static Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		private static Config GetConfig()
		{
			if (File.Exists("PlayerStatsBot.json"))
				return JsonConvert.DeserializeObject<Config>(File.ReadAllText("PlayerStatsBot.json"));
			File.WriteAllText("PlayerStatsBot.json", JsonConvert.SerializeObject(Config.Default, (Formatting) 1));
			return Config.Default;
		}
	}
}