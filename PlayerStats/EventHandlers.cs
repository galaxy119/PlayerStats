using System;
using System.Collections.Generic;
using EXILED;
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
			for (; ; )
			{
				foreach (Stats stats in Stats.Values)
					stats.SecondsPlayed++;

				yield return Timing.WaitForSeconds(1f);
			}
		}

		public void OnRoundEnd()
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

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			if (string.IsNullOrEmpty(ev.Player.characterClassManager.UserId) || ev.Player.characterClassManager.IsHost || ev.Player.nicknameSync.MyNick == "Dedicated Server")
				return;

			if (!Stats.ContainsKey(ev.Player.characterClassManager.UserId))
				Stats.Add(ev.Player.characterClassManager.UserId, Methods.LoadStats(ev.Player.characterClassManager.UserId));
		}

		public void OnPlayerDeath(ref PlayerDeathEvent ev)
		{
			Log.Info("Player death event..");
			if (ev.Player == null || string.IsNullOrEmpty(ev.Player.characterClassManager.UserId))
				return;

			Log.Info($"Player: {ev.Player.nicknameSync.MyNick} {ev.Player.characterClassManager.UserId}");
			if (Stats.ContainsKey(ev.Player.characterClassManager.UserId))
			{
				string killer = ev.Info.Attacker;
				if (ev.Info.Attacker.Contains("anti-cheat")) 
				{
					killer = "AntiCheat";
				}
				Log.Info($"Adding stats to {ev.Player.characterClassManager.UserId}");
				Log.Info($"Attacker info for {ev.Player.characterClassManager.UserId} - {killer}");
				Stats[ev.Player.characterClassManager.UserId].Deaths++;
				Stats[ev.Player.characterClassManager.UserId].LastKiller = killer;
				if (ev.Killer == null || ev.Player == ev.Killer ||
					string.IsNullOrEmpty(ev.Killer.characterClassManager.UserId))
				{
					Log.Info($"Counting as suicide..{ev.Player.characterClassManager.UserId}");
					Stats[ev.Player.characterClassManager.UserId].Suicides++;
					return;
				}
			}

			if (ev.Killer == null || string.IsNullOrEmpty(ev.Killer.characterClassManager.UserId))
				return;
			Log.Info($"Attacker: {ev.Killer.nicknameSync.MyNick} - {ev.Killer.characterClassManager.UserId}");
			if (Stats.ContainsKey(ev.Killer.characterClassManager.UserId))
			{
				Log.Debug($"Adding stats for killer {ev.Killer.characterClassManager.UserId}");
				Stats[ev.Killer.characterClassManager.UserId].Kills++;
				Stats[ev.Killer.characterClassManager.UserId].LastVictim =
					ev.Player.nicknameSync.MyNick + $"({ev.Player.characterClassManager.UserId})";

				if (!ev.Killer.characterClassManager.IsHuman())
				{
					Log.Info($"{ev.Killer.characterClassManager.UserId} is not human, counting as SCP kill. {ev.Killer.characterClassManager.CurClass}");
					Stats[ev.Killer.characterClassManager.UserId].ScpKills++;
				}
			}
		}

		public void OnThrowGrenade(ref GrenadeThrownEvent ev)
		{
			GrenadeSettings settings = ev.Gm.availableGrenades[ev.Id];
			ItemType type = settings.inventoryID;

			if (type == ItemType.SCP018 && Stats.ContainsKey(ev.Player.characterClassManager.UserId))
				Stats[ev.Player.characterClassManager.UserId].Scp018Throws++;
		}

		public void OnMedicalItem(MedicalItemEvent ev)
		{
			if (ev.Item == ItemType.SCP207 && Stats.ContainsKey(ev.Player.characterClassManager.UserId))
				Stats[ev.Player.characterClassManager.UserId].Scp207Uses++;
		}
	}
}
