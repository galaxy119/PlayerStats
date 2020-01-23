using System;
using System.IO;
using EXILED;

namespace PlayerStats
{
	public class Plugin : EXILED.Plugin
	{
		public EventHandlers EventHandlers;

		internal static string StatFilePath =
			Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Plugins"),
				"PlayerStats");

		public override void OnEnable()
		{
			if (!Directory.Exists(StatFilePath))
				Directory.CreateDirectory(StatFilePath);
			
			Info($"Loading {getName}..");
			EventHandlers = new EventHandlers(this);
			Events.WaitingForPlayersEvent += EventHandlers.OnWaitingForPlayers;
			Events.RoundStartEvent += EventHandlers.OnRoundStart;
			Events.RoundEndEvent += EventHandlers.OnRoundEnd;
			Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;
			Events.PlayerDeathEvent += EventHandlers.OnPlayerDeath;
			Events.GrenadeThrownEvent += EventHandlers.OnThrowGrenade;
			Events.UseMedicalItemEvent += EventHandlers.OnMedicalItem;
			
			Info($"{getName} loaded.");
		}

		public override void OnDisable()
		{
			Events.WaitingForPlayersEvent -= EventHandlers.OnWaitingForPlayers;
			Events.RoundStartEvent -= EventHandlers.OnRoundStart;
			Events.RoundEndEvent -= EventHandlers.OnRoundEnd;
			Events.PlayerJoinEvent -= EventHandlers.OnPlayerJoin;
			Events.PlayerDeathEvent -= EventHandlers.OnPlayerDeath;
			Events.GrenadeThrownEvent -= EventHandlers.OnThrowGrenade;
			Events.UseMedicalItemEvent -= EventHandlers.OnMedicalItem;
			EventHandlers = null;
		}

		public override void OnReload()
		{
			
		}

		public override string getName { get; } = "Player Stats";
	}
}