using System.Collections.Generic;
using System.IO;

namespace PlayerStats
{
	public class Methods
	{
		internal static Stats LoadStats(string userId)
		{
			string path = Path.Combine(Plugin.StatFilePath, $"{userId}.txt");

			if (File.Exists(path))
				return DeserializeStats(path);
			else
			{
				File.Create(path).Close();
				return new Stats()
				{
					UserId = userId,
					RoundsPlayed = 1,
					Deaths = 0,
					Suicides = 0,
					Escapes = 0,
					Kills = 0,
					Krd = 0,
					LastKiller = string.Empty,
					LastVictim = string.Empty,
					Scp018Throws = 0,
					Scp207Uses = 0,
					ScpKills = 0,
				};
			}
		}

		internal static void SaveStats(Stats stats)
		{
			stats.Krd = (double) stats.Kills / stats.Deaths;
			string[] write = new[]
			{
				stats.UserId, 
				stats.SecondsPlayed.ToString(), 
				stats.Kills.ToString(), 
				stats.ScpKills.ToString(), 
				stats.Deaths.ToString(),
				stats.Suicides.ToString(), 
				stats.Scp207Uses.ToString(), 
				stats.Scp018Throws.ToString(), 
				stats.Krd.ToString(),
				stats.LastKiller, 
				stats.LastVictim, 
				stats.Escapes.ToString(), 
				stats.RoundsPlayed.ToString()
			};

			string path = Path.Combine(Plugin.StatFilePath, $"{stats.UserId}.txt");
			File.WriteAllLines(path, write);
		}

		private static Stats DeserializeStats(string path)
		{
			string[] read = File.ReadAllLines(path);
			Stats stats = new Stats
			{
				UserId = read[0],
				SecondsPlayed = float.Parse(read[1]),
				Kills = int.Parse(read[2]),
				ScpKills = int.Parse(read[3]),
				Deaths = int.Parse(read[4]),
				Suicides = int.Parse(read[5]),
				Scp207Uses = int.Parse(read[6]),
				Scp018Throws = int.Parse(read[7]),
				Krd = double.Parse(read[8]),
				LastKiller = read[9],
				LastVictim = read[10],
				Escapes = int.Parse(read[11]),
				RoundsPlayed = int.Parse(read[12])
			};
			return stats;
		}
	}
}