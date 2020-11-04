using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Grenades;
using MEC;

namespace PlayerStats
{
	public class EventHandlers
	{
		private readonly Plugin plugin;
		public EventHandlers(Plugin plugin) => this.plugin = plugin;
		public Dictionary<string, Stats> Stats = new Dictionary<string, Stats>();
		public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

		public void OnWaitingForPlayers()
		{
			Stats.Clear();
			foreach (CoroutineHandle handle in Coroutines)
				Timing.KillCoroutines(handle);
		}

		public void OnRoundStart()
		{
			foreach (Stats stats in Stats.Values)
				stats.RoundsPlayed++;
			Coroutines.Add(Timing.RunCoroutine(SecondCounter()));
		}

		private IEnumerator<float> SecondCounter()
		{
			for (;;)
			{
				foreach (Stats stats in Stats.Values)
					stats.SecondsPlayed++;

				yield return Timing.WaitForSeconds(1f);
			}
		}

		public void OnRoundEnd(RoundEndedEventArgs ev)
		{
			Log.Debug("Round is ending.");
			foreach (CoroutineHandle handle in Coroutines)
				Timing.KillCoroutines(handle);
			try
			{
				foreach (Stats stats in Stats.Values)
					Methods.SaveStats(stats);
			}
			catch (Exception e)
			{
				Log.Error($"Round End error: {e}");
			}
		}

		public void OnPlayerJoin(JoinedEventArgs ev)
		{
			try
			{
				if (string.IsNullOrEmpty(ev.Player.UserId) || ev.Player.IsHost || ev.Player.Nickname == "Dedicated Server")
					return;

				if (!Stats.ContainsKey(ev.Player.UserId))
					Stats.Add(ev.Player.UserId,
						Methods.LoadStats(ev.Player.UserId));
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		public void OnPlayerDeath(DiedEventArgs ev)
		{
			Log.Debug("Player death event..", plugin.Config.Debug);
			if (ev.Target == null || string.IsNullOrEmpty(ev.Target.UserId))
				return;
			
			Log.Debug($"Player: {ev.Target.Nickname} {ev.Target.UserId}", plugin.Config.Debug);
			if (Stats.ContainsKey(ev.Target.UserId))
			{
				Log.Debug($"Adding stats to {ev.Target.UserId}", plugin.Config.Debug);
				Log.Debug($"Attacker info for {ev.Target.UserId} - {ev.Killer.UserId}", plugin.Config.Debug);
				Stats[ev.Target.UserId].Deaths++;
				if (ev.Killer.Nickname.ToLower().Contains("anti-cheat") || ev.Killer.Nickname.ToLower().Contains("anticheat"))
					Stats[ev.Target.UserId].LastKiller = "Anticheat";
				else
					Stats[ev.Target.UserId].LastKiller = ev.Killer.Nickname;
				if (ev.Killer == null || ev.Target == ev.Killer || string.IsNullOrEmpty(ev.Killer.UserId))
				{
					Log.Debug($"Counting as suicide..{ev.Target.UserId}", plugin.Config.Debug);
					Stats[ev.Target.UserId].Suicides++;
					return;
				}
			}

			if (ev.Killer == null || string.IsNullOrEmpty(ev.Killer.UserId))
				return;
			Log.Debug($"Attacker: {ev.Killer.Nickname} - {ev.Killer.UserId}", plugin.Config.Debug);
			if (Stats.ContainsKey(ev.Killer.UserId))
			{
				Log.Debug($"Adding stats for killer {ev.Killer.UserId}", plugin.Config.Debug);
				Stats[ev.Killer.UserId].Kills++;
				Stats[ev.Killer.UserId].LastVictim =
					ev.Target.Nickname + $"({ev.Target.UserId})";

				if (ev.Killer.Team == Team.SCP)
				{
					Log.Debug($"{ev.Killer.UserId} is not human, counting as SCP kill. {ev.Killer.Role}", plugin.Config.Debug);
					Stats[ev.Killer.UserId].ScpKills++;
				}
			}
		}

		public void OnThrowGrenade(ThrowingGrenadeEventArgs ev)
		{
			GrenadeSettings settings = ev.GrenadeManager.availableGrenades[(int)ev.Type];
			ItemType type = settings.inventoryID;

			if (type == ItemType.SCP018 && Stats.ContainsKey(ev.Player.UserId))
				Stats[ev.Player.UserId].Scp018Throws++;
		}

		public void OnMedicalItem(UsedMedicalItemEventArgs ev)
		{
			if (ev.Item == ItemType.SCP207 && Stats.ContainsKey(ev.Player.UserId))
				Stats[ev.Player.UserId].Scp207Uses++;
		}
	}
}