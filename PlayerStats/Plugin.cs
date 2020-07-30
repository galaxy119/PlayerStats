using System;
using System.IO;
using Exiled.API.Features;
using Exiled.Events;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;

namespace PlayerStats
{
	public class Plugin : Exiled.API.Features.Plugin<Config>
	{
		public EventHandlers EventHandlers;

		internal static string StatFilePath =
			Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED"), "Plugins"),
				"PlayerStats");

		public override void OnEnabled()
		{
			if (!Directory.Exists(StatFilePath))
				Directory.CreateDirectory(StatFilePath);
			
			EventHandlers = new EventHandlers(this);
			Server.WaitingForPlayers += EventHandlers.OnWaitingForPlayers;
			Server.RoundStarted += EventHandlers.OnRoundStart;
			Server.RoundEnded += EventHandlers.OnRoundEnd;
			Player.Joined += EventHandlers.OnPlayerJoin;
			Player.Died += EventHandlers.OnPlayerDeath;
			Player.ThrowingGrenade += EventHandlers.OnThrowGrenade;
			Player.MedicalItemUsed += EventHandlers.OnMedicalItem;
		}

		public override void OnDisabled()
		{
			Server.WaitingForPlayers -= EventHandlers.OnWaitingForPlayers;
			Server.RoundStarted -= EventHandlers.OnRoundStart;
			Server.RoundEnded -= EventHandlers.OnRoundEnd;
			Player.Joined -= EventHandlers.OnPlayerJoin;
			Player.Died -= EventHandlers.OnPlayerDeath;
			Player.ThrowingGrenade -= EventHandlers.OnThrowGrenade;
			Player.MedicalItemUsed -= EventHandlers.OnMedicalItem;
			EventHandlers = null;
		}
	}
}
