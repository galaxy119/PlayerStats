using System;

namespace PlayerStatsBot
{
	[Serializable]
	public class Stats
	{
		public string UserId;
		public float SecondsPlayed;
		public int Kills;
		public int ScpKills;
		public int Deaths;
		public int Suicides;
		public int Scp207Uses;
		public int Scp018Throws;
		public double Krd;
		public string LastKiller;
		public string LastVictim;
		public int Escapes;
		public int RoundsPlayed;
	}
}