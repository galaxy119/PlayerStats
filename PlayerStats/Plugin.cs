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
		public override string Author { get; } = "galaxy119";
		public override Version Version { get; } = new Version(2, 0,0);
		public override Version RequiredExiledVersion { get; } = new Version(2,1,12);
		public EventHandlers EventHandlers;

		internal static string StatFilePath = Path.Combine(Paths.Plugins, "PlayerStats");

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