namespace PlayerStatsBot
{
	public class Config
	{
		public static readonly Config Default = new Config
		{
			BotPrefix = "!",
			BotToken = "",
			SyncFile = "DiscordSteamSyncs.yml",
			StatsFile = "/home/scp/.config/Plugins/PlayerStats",
			StaffID = 0
		};

		public string BotPrefix { get; set; }

		public string BotToken { get; set; }

		public string SyncFile { get; set; }

		public string StatsFile { get; set; }

		public ulong StaffID { get; set; }
	}
}