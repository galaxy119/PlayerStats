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

		public void OnRestartingRound()
		{
			Log.Debug("Round is restarting.");
			foreach (CoroutineHandle handle in Coroutines)
				Timing.KillCoroutines(handle);
			try
			{
				foreach (Stats stats in Stats.Values)
					Methods.SaveStats(stats);
			}
			catch (Exception e)
			{
				Log.Error($"Round Restart error: {e}");
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
			Log.Info("Player death event..");
			if (ev.Target == null || string.IsNullOrEmpty(ev.Target.UserId))
				return;
			
			Log.Info($"Player: {ev.Target.Nickname} {ev.Target.UserId}");
			if (Stats.ContainsKey(ev.Target.UserId))
			{
				Log.Info($"Adding stats to {ev.Target.UserId}");
				Log.Info($"Attacker info for {ev.Target.UserId} - {ev.Killer.UserId}");
				Stats[ev.Target.UserId].Deaths++;
				Stats[ev.Target.UserId].LastKiller = ev.Killer.Nickname;
				if (ev.Killer == null || ev.Target == ev.Killer ||
				    string.IsNullOrEmpty(ev.Killer.UserId))
				{
					Log.Info($"Counting as suicide..{ev.Target.UserId}");
					Stats[ev.Target.UserId].Suicides++;
					return;
				}
			}

			if (ev.Killer == null || string.IsNullOrEmpty(ev.Killer.UserId))
				return;
			Log.Info($"Attacker: {ev.Killer.Nickname} - {ev.Killer.UserId}");
			if (Stats.ContainsKey(ev.Killer.UserId))
			{
				Log.Debug($"Adding stats for killer {ev.Killer.UserId}");
				Stats[ev.Killer.UserId].Kills++;
				Stats[ev.Killer.UserId].LastVictim =
					ev.Target.Nickname + $"({ev.Target.UserId})";

				if (ev.Killer.Team == Team.SCP)
				{
					Log.Info($"{ev.Killer.UserId} is not human, counting as SCP kill. {ev.Killer.Role}");
					Stats[ev.Killer.UserId].ScpKills++;
				}
			}
		}

		public void OnThrowGrenade(ThrowingGrenadeEventArgs ev)
		{
			GrenadeSettings settings = ev.GrenadeManager.availableGrenades[ev.Id];
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
