using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace PlayerStatsBot
{
  public class Bot
  {
    private readonly Deserializer yamlDeserializer = new Deserializer();
    private static DiscordSocketClient _client;
    private readonly Program program;

    private static DiscordSocketClient Client => _client ?? (_client = new DiscordSocketClient());

    public Bot(Program program)
    {
      this.program = program;
      InitBot().GetAwaiter().GetResult();
    }

    private async Task InitBot()
    {
      Client.Log += Program.Log;
      Client.MessageReceived += OnMessageReceived;
      await Client.LoginAsync(TokenType.Bot, program.Config.BotToken);
      await Client.StartAsync();
      if (!File.Exists(program.Config.SyncFile))
        File.Create(program.Config.SyncFile).Close();
      await Task.Delay(-1);
    }

    private async Task OnMessageReceived(SocketMessage msg)
    {
      if (!msg.Content.StartsWith(program.Config.BotPrefix))
        return;
      CommandContext context = new CommandContext(Client, (IUserMessage) msg);
      HandleCommand(context);
    }

    private async Task HandleCommand(ICommandContext context)
    {
      if (context.Message.Content.ToLower().Contains("ping"))
      {
        IUserMessage userMessage = await context.Channel.SendMessageAsync("Pong!");
      }
      else if (context.Message.Content.ToLower().Contains("desync"))
        await DoDesyncAsync(context);
      else if (context.Message.Content.ToLower().Contains("sync"))
        await DoSyncAsync(context);
      else
      {
        if (!context.Message.Content.ToLower().Contains("stats"))
          return;
        if (context.Message.Content.ToLower().Contains("tesla"))
        {
          await CheckTeslaStats(context);
          return;
        }
        await CheckStats(context);
      }
    }

    private async Task DoSyncAsync(ICommandContext context)
    {
      string[] args = context.Message.Content.Split(' ');
      try
      {
        if (!string.IsNullOrEmpty(File.ReadAllText(program.Config.SyncFile)))
        {
          StreamReader reader = new StreamReader(program.Config.SyncFile);
          StringReader input = new StringReader(reader.ReadToEnd());
          reader.Close();
          YamlStream yaml = new YamlStream();
          yaml.Load(input);
          YamlMappingNode mapping = (YamlMappingNode) yaml.Documents[0].RootNode;
          Sync[] syncs = mapping.Children.Select(entry => new Sync
          {
            DiscordId = ulong.Parse(entry.Key.ToString()), SteamId = entry.Value.ToString()
          }).ToArray();
          if (syncs.Length != 0 && syncs.Any(s => (long) s.DiscordId == (long) context.Message.Author.Id))
            await context.Channel.SendMessageAsync("Your Discord is already synced!");
          else
          {
            if (args.Length == 1 || !ulong.TryParse(args[1], out ulong _))
              await context.Channel.SendMessageAsync("Please provide a valid SteamID.");
            else
            {
              string newSync = ((long) context.Message.Author.Id) + ": '" + args[1] + "@steam'";
              File.AppendAllText(program.Config.SyncFile, newSync + Environment.NewLine);
              await context.Channel.SendMessageAsync("@" + context.Message.Author +
                                                     " Your Discord is now synced with the provided SteamID!");
            }
          }
        }
        else
        {
          string newSync = ((long) context.Message.Author.Id) + ": '" + args[1] + "@steam'";
          File.AppendAllText(program.Config.SyncFile, newSync + Environment.NewLine);
          await context.Channel.SendMessageAsync("@" + context.Message.Author +
                                                 " Your Discord is now synced with the provided SteamID!");
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }

    private async Task DoDesyncAsync(ICommandContext context)
    {
      StreamReader reader = new StreamReader(program.Config.SyncFile);
      StringReader input = new StringReader(reader.ReadToEnd());
      reader.Close();
      YamlStream yaml = new YamlStream();
      yaml.Load(input);
      YamlMappingNode mapping = (YamlMappingNode) yaml.Documents[0].RootNode;
      Sync[] syncs = mapping.Children.Select(entry => new Sync
      {
        DiscordId = ulong.Parse(entry.Key.ToString()),
        SteamId = entry.Value.ToString()
      }).ToArray();
      Sync[] syncArray = syncs;
      for (int index = 0; index < syncArray.Length; ++index)
      {
        Sync syn = syncArray[index];
        Console.WriteLine((long) syn.DiscordId + ": " + syn.SteamId);
      }

      if (syncs.All(s => (long) s.DiscordId != (long) context.Message.Author.Id))
        await context.Channel.SendMessageAsync("Your Discord is not synced!");
      else
      {
        string[] oldLines = File.ReadAllLines(program.Config.SyncFile);
        IEnumerable<string> newLines = oldLines.Where(l => !l.Contains(context.Message.Author.Id.ToString()));
        File.WriteAllLines(program.Config.SyncFile, newLines);
        await context.Channel.SendMessageAsync("@" + context.Message.Author + " Your Discord is now unsynced!");
      }
    }

    private async Task CheckTeslaStats(ICommandContext context)
    {
      string path = "/home/scp/scp_server/tesla.txt";
      if (!File.Exists(path))
      {
        await context.Channel.SendMessageAsync("Error: Tesla stats file not found!");
        return;
      }

      string[] read = File.ReadAllLines(path);
      Tesla tesla = new Tesla
      {
        Fires = int.Parse(read[0]),
        Kills = int.Parse(read[1]),
        Dboikills = int.Parse(read[2]),
        NerdKills = int.Parse(read[3]),
        CiKills = int.Parse(read[4]),
        NtfKills = int.Parse(read[5]),
        ScpKills = int.Parse(read[6])
      };
      
      EmbedBuilder embed = new EmbedBuilder();
      embed.WithColor(Color.Blue);
      embed.WithFooter("Joker's Playground Stats bot - Joker11");
      embed.WithCurrentTimestamp();
      embed.WithTitle("Joker's Playground Tesla Gate Stats");
      embed.AddField("Times Fired", tesla.Fires, true);
      embed.AddField("Total Kills", tesla.Kills, true);
      embed.AddField("Dboi Kills", tesla.Dboikills, true);
      embed.AddField("Nerd Kills", tesla.NerdKills, true);
      embed.AddField("Chaos Insurgency Kills", tesla.CiKills, true);
      embed.AddField("NTF Kills", tesla.NtfKills, true);
      embed.AddField("SCP Kills", tesla.ScpKills, true);

      await context.Channel.SendMessageAsync(null, false, embed.Build());
    }

    private async Task CheckStats(ICommandContext context)
    {
      try
      {
        StreamReader reader = new StreamReader(program.Config.SyncFile);
        StringReader input = new StringReader(reader.ReadToEnd());
        reader.Close();
        YamlStream yaml = new YamlStream();
        yaml.Load(input);
        YamlMappingNode mapping = (YamlMappingNode) yaml.Documents[0].RootNode;
        Sync[] syncs = mapping.Children.Select(entry => new Sync
        {
          DiscordId = ulong.Parse(entry.Key.ToString()),
          SteamId = entry.Value.ToString()
        }).ToArray();
        string[] args = context.Message.Content.Split(new string[1]
        {
          " "
        }, StringSplitOptions.None);
        Console.WriteLine(args.Length);
        Stats stats;
        EmbedBuilder embed;
        string steamId;
        string path;
        string steamName;
        ulong result1;
        if (args.Length > 1 && !ulong.TryParse(args[1], out result1))
        {
          string name = args[1];
          if (args[1].Contains("@"))
          {
            name = args[1].Replace("@", "");
            name = name.Replace("<", "");
            name = name.Replace(">", "");
            if (name.Contains("!"))
              name = name.Replace("!", "");
          }
          IEnumerable<IGuildUser> users = context.Guild.GetUsersAsync(CacheMode.CacheOnly).Result.Where(u =>
          {
            ulong result;
            if (!ulong.TryParse(name, out result))
              return u.Username == name;
            return (long) u.Id == (long) result;
          });
          IGuildUser user = users.OrderBy(u => u.Username.Length).First();
          if (user != null)
          {
            ulong id = user.Id;
            if (syncs.All(s => (long) s.DiscordId != (long) id))
              await context.Channel.SendMessageAsync("User is not synced!");
            else
            {
              steamId = syncs[Array.FindIndex(syncs, s => (long) s.DiscordId == (long) id)].SteamId;
              path = program.Config.StatsFile + "/" + steamId + ".txt";
              Console.WriteLine(path);
              if (!File.Exists(path))
                await context.Channel.SendMessageAsync("No stats found.");
              else
              {
                steamName = await GetSteamName(steamId.Replace("@steam",""));
                stats = DeserializeStats(path);
                embed = new EmbedBuilder();
                if (string.IsNullOrEmpty(stats.LastKiller))
                  stats.LastKiller = "None";
                if (string.IsNullOrEmpty(stats.LastVictim))
                  stats.LastVictim = "None";
                embed.WithTitle("Joker's Playground Player Stats for: " + steamName + "(" + steamId + ")");
                embed.AddField("Total Kills", stats.Kills, true);
                embed.AddField("Total SCP Kills", stats.ScpKills, true);
                embed.AddField("Deaths", stats.Deaths, true);
                embed.AddField("Suicides", stats.Suicides, true);
                embed.AddField("SCP-207 Drinks", stats.Scp207Uses, true);
                embed.AddField("SCP-018 Throws", stats.Scp018Throws, true);
                embed.AddField("Last Killed", stats.LastKiller, true);
                embed.AddField("Last Victim", stats.LastVictim, true);
                embed.AddField("KDR", stats.Krd, true);
                embed.AddField("Escapes Made", stats.Escapes, true);
                embed.AddField("Rounds Played", stats.RoundsPlayed);
                embed.AddField("Time Played", GetHRT(stats.SecondsPlayed));
                embed.WithFooter(f => f.Text = "Joker's Playground Stats Bot by Joker119").WithColor(Color.Blue)
                  .WithCurrentTimestamp();
                await context.Channel.SendMessageAsync(null, false, embed.Build());
              }
            }
          }
          else
            await context.Channel.SendMessageAsync("User not found.");
        }
        else if (args.Length > 1 && ulong.TryParse(args[1], out result1))
        {
          steamId = args[1] + "@steam";
          path = program.Config.StatsFile + "/" + steamId + ".txt";
          Console.WriteLine(path);
          if (!File.Exists(path))
            await context.Channel.SendMessageAsync("No stats found.");
          else
          {
            steamName = await GetSteamName(steamId.Replace("@steam",""));
            stats = DeserializeStats(path);
            embed = new EmbedBuilder();
            if (string.IsNullOrEmpty(stats.LastKiller))
              stats.LastKiller = "None";
            if (string.IsNullOrEmpty(stats.LastVictim))
              stats.LastVictim = "None";
            embed.WithTitle("Joker's Playground Player Stats for: " + steamName + "(" + steamId + ")");
            embed.AddField("Total Kills", stats.Kills, true);
            embed.AddField("Total SCP Kills", stats.ScpKills, true);
            embed.AddField("Deaths", stats.Deaths, true);
            embed.AddField("Suicides", stats.Suicides, true);
            embed.AddField("SCP-207 Drinks", stats.Scp207Uses, true);
            embed.AddField("SCP-018 Throws", stats.Scp018Throws, true);
            embed.AddField("Last Killed", stats.LastKiller, true);
            embed.AddField("Last Victim", stats.LastVictim, true);
            embed.AddField("KDR", stats.Krd, true);
            embed.AddField("Escapes Made", stats.Escapes, true);
            embed.AddField("Rounds Played", stats.RoundsPlayed);
            embed.AddField("Time Played", GetHRT(stats.SecondsPlayed));
            embed.WithFooter(f => f.Text = "Joker's Playground Stats Bot by Joker119").WithColor(Color.Blue)
              .WithCurrentTimestamp();
            await context.Channel.SendMessageAsync("", false, embed.Build());
          }
        }
        else if (syncs.All(s => (long) s.DiscordId != (long) context.Message.Author.Id))
          await context.Channel.SendMessageAsync("Your discord is not synced!");
        else
        {
          steamId = syncs[Array.FindIndex(syncs, s => (long) s.DiscordId == (long) context.Message.Author.Id)].SteamId;
          path = program.Config.StatsFile + "/" + steamId + ".txt";
          Console.WriteLine(path);
          if (!File.Exists(path))
            await context.Channel.SendMessageAsync("No stats found.");
          else
          {
            steamName = await GetSteamName(steamId.Replace("@steam",""));
            stats = DeserializeStats(path);
            embed = new EmbedBuilder();
            if (string.IsNullOrEmpty(stats.LastKiller))
              stats.LastKiller = "None";
            if (string.IsNullOrEmpty(stats.LastVictim))
              stats.LastVictim = "None";
            embed.WithTitle("Joker's Playground Player Stats for: " + steamName + "(" + steamId + ")");
            embed.AddField("Total Kills", stats.Kills, true);
            embed.AddField("Total SCP Kills", stats.ScpKills, true);
            embed.AddField("Deaths", stats.Deaths, true);
            embed.AddField("Suicides", stats.Suicides, true);
            embed.AddField("SCP-207 Drinks", stats.Scp207Uses, true);
            embed.AddField("SCP-018 Throws", stats.Scp018Throws, true);
            embed.AddField("Last Killed", stats.LastKiller, true);
            embed.AddField("Last Victim", stats.LastVictim, true);
            embed.AddField("KDR", stats.Krd, true);
            embed.AddField("Escapes Made", stats.Escapes, true);
            embed.AddField("Rounds Played", stats.RoundsPlayed);
            embed.AddField("Time Played", GetHRT(stats.SecondsPlayed));
            embed.WithFooter(f => f.Text = "Joker's Playground Stats Bot by Joker119").WithColor(Color.Blue)
              .WithCurrentTimestamp();
            await context.Channel.SendMessageAsync("", false, embed.Build());
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
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

    private static string GetHRT(float time)
    {
      time /= 60;
      TimeSpan timeSpan = TimeSpan.FromMinutes(time);
      return
        $"{(object) timeSpan.Days} days {(object) timeSpan.Hours} hours {(object) timeSpan.Minutes} minutes {(object) timeSpan.Seconds} seconds.";
    }

    private static async Task<string> GetSteamName(string steamId)
    {
      string apikey = "4C3DD54D9D92C18291F05FF2F59639A4";
      await DownloadWebClient(out string result, "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + apikey + "&steamids=" + steamId);
      RootObject players = JsonConvert.DeserializeObject<RootObject>(result);
      SteamUser player = players.response.players.FirstOrDefault();
      return player?.personaname;
    }

    private static Task DownloadWebClient(out string result, string url)
    {
      using (WebClient webClient = new WebClient())
      {
        webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
        result = webClient.DownloadString(url);
      }
      return Task.FromResult(1);
    }
  }
}
