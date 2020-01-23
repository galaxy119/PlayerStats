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
			for (;;)
			{
				foreach (Stats stats in Stats.Values)
					stats.SecondsPlayed++;

				yield return Timing.WaitForSeconds(1f);
			}
		}

		public void OnRoundEnd()
		{
			foreach (CoroutineHandle handle in Coroutines)
				Timing.KillCoroutines(handle);
			foreach (Stats stats in Stats.Values)
				Methods.SaveStats(stats);
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			Stats.Add(ev.Player.characterClassManager.UserId, Methods.LoadStats(ev.Player.characterClassManager.UserId));
		}

		public void OnPlayerDeath(ref PlayerDeathEvent ev)
		{
			if (Stats.ContainsKey(ev.Player.characterClassManager.UserId))
			{
				Stats[ev.Player.characterClassManager.UserId].Deaths++;
				Stats[ev.Player.characterClassManager.UserId].LastKiller = ev.Info.Attacker;
				if (ev.Killer == null || ev.Player == ev.Killer ||
				    string.IsNullOrEmpty(ev.Killer.characterClassManager.UserId))
				{
					Stats[ev.Player.characterClassManager.UserId].Suicides++;
					return;
				}
			}

			if (Stats.ContainsKey(ev.Killer.characterClassManager.UserId))
			{
				Stats[ev.Killer.characterClassManager.UserId].Kills++;
				Stats[ev.Killer.characterClassManager.UserId].LastVictim =
					ev.Player.nicknameSync.MyNick + $"({ev.Player.characterClassManager.UserId})";
				
				if (!ev.Killer.characterClassManager.IsHuman())
					Stats[ev.Killer.characterClassManager.UserId].ScpKills++;
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