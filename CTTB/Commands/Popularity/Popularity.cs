﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using IronPython.Hosting;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CTTB.Commands
{
    public class Popularity : BaseCommandModule
    {
        [Command("update")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task Update(CommandContext ctx)
        {
            string rtttUrl = "http://tt.chadsoft.co.uk/original-track-leaderboards.json";
            string ctttUrl = "http://tt.chadsoft.co.uk/ctgp-leaderboards.json";
            string rttt200Url = "http://tt.chadsoft.co.uk/original-track-leaderboards-200cc.json";
            string cttt200Url = "http://tt.chadsoft.co.uk/ctgp-leaderboards-200cc.json";
            string ctwwUrl = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0";
            string wwUrl = "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0";

            // Leaderboards

            var rtRawJson = JsonConvert.DeserializeObject<LeaderboardInfo>(new WebClient().DownloadString(rtttUrl));

            var ctRawJson = JsonConvert.DeserializeObject<LeaderboardInfo>(new WebClient().DownloadString(ctttUrl));

            var rtRaw200Json = JsonConvert.DeserializeObject<LeaderboardInfo>(new WebClient().DownloadString(rttt200Url));

            var ctRaw200Json = JsonConvert.DeserializeObject<LeaderboardInfo>(new WebClient().DownloadString(cttt200Url));

            var rtJson = JsonConvert.SerializeObject(rtRawJson.Leaderboard);
            File.WriteAllText("rts.json", rtJson);

            var ctJson = JsonConvert.SerializeObject(ctRawJson.Leaderboard);
            File.WriteAllText("cts.json", ctJson);

            var rt200Json = JsonConvert.SerializeObject(rtRaw200Json.Leaderboard);
            File.WriteAllText("rts200.json", rt200Json);

            var ct200Json = JsonConvert.SerializeObject(ctRaw200Json.Leaderboard);
            File.WriteAllText("cts200.json", ct200Json);

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = $"__**Notice:**__",
                Description = "Database has been updated.",
                Timestamp = DateTime.UtcNow
            };

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("besttime")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task GetBestTimes(CommandContext ctx, string trackType = "rts", [RemainingText] string track = "")
        {
            string json = "";
            string description = "";

            var embed = new DiscordEmbedBuilder { };

            try
            {
                json = File.ReadAllText($"{trackType}.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                int j = 0;
                List<Track> trackDisplay = new List<Track>();

                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Name.ToLowerInvariant().Contains(track.ToLowerInvariant()))
                    {
                        trackDisplay.Add(trackList[i]);
                        j++;
                    }
                }

                if (j < 1)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*" +
                        "\n**c!besttime [rts/cts/rts200/cts200] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else if (j == 1)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Best Time on {track}:**__",
                        Description = $"{trackDisplay[0].Name} - *{trackDisplay[0].BestTime}*",
                        Timestamp = DateTime.UtcNow
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    if (trackDisplay[0].Category == 16)
                    {
                        for (int i = 0; i < trackDisplay.Count; i++)
                        {
                            if (trackDisplay[i].Category == 16)
                            {
                                description += $"{trackDisplay[i].Name} (Shortcut) - *{trackDisplay[i].BestTime}*\n";
                            }
                            if (trackDisplay[i].Category == 1 || trackDisplay[i].Category == 5)
                            {
                                description += $"{trackDisplay[i].Name} (Glitch) - *{trackDisplay[i].BestTime}*\n";
                            }
                            if (trackDisplay[i].Category == 2 || trackDisplay[i].Category == 6)
                            {
                                description += $"{trackDisplay[i].Name} (No Shortcut) - *{trackDisplay[i].BestTime}*\n";
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < trackDisplay.Count; i++)
                        {
                            if (trackDisplay[i].Category == 0 || trackDisplay[i].Category == 4)
                            {
                                description += $"{trackDisplay[i].Name} (No Shortcut) - *{trackDisplay[i].BestTime}*\n";
                            }
                            if (trackDisplay[i].Category == 1 || trackDisplay[i].Category == 5)
                            {
                                description += $"{trackDisplay[i].Name} (Glitch) - *{trackDisplay[i].BestTime}*\n";
                            }
                            if (trackDisplay[i].Category == 2 || trackDisplay[i].Category == 6)
                            {
                                description += $"{trackDisplay[i].Name} (Shortcut) - *{trackDisplay[i].BestTime}*\n";
                            }
                        }
                    }
                    if (track.Length < 3)
                    {

                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = "*Please input a track name to filter with (min 3 chars).*" +
                               "\n**c!besttime [rts/cts/rts200/cts200] [name of track]**",
                            Timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Best Times on {track}:**__",
                            Description = description,
                            Timestamp = DateTime.UtcNow
                        };
                    }
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                }
            }
            catch
            {
                if (trackType != "rts" && trackType != "cts" && trackType != "rts200" && trackType != "cts200")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track type is not valid.*" +
                        "\n**c!besttime [rts/cts/rts200/cts200] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Unknown Error.*" +
                        "\n**c!besttime [rts/cts/rts200/cts200] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("wwpop")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task WWPopularityRequest(CommandContext ctx, string trackType = "rts", int display = 0)
        {
            var embed = new DiscordEmbedBuilder { };

            string json = "";

            if (trackType == "rts")
            {
                if (display < 1)
                {
                    display = 1;
                }
                else if (display > 12)
                {
                    display = 12;
                }
            }
            else if (trackType == "cts")
            {
                if (display < 1)
                {
                    display = 1;
                }
                else if (display > 198)
                {
                    display = 198;
                }
            }

            try
            {
                json = File.ReadAllText($"{trackType}.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);
                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Category % 16 != 0)
                    {
                        trackList.RemoveAt(i);
                        i--;
                    }
                }
                trackList = trackList.OrderBy(a => a.WiimmfiScore).ToList();
                trackList.Reverse();

                string description = "";

                for (int i = display - 1; i < display + 20; i++)
                {
                    description = description + $"**{i + 1})** {trackList[i].Name} *({trackList[i].WiimmfiScore})*\n";
                }

                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Displaying {display} - {display + 20}:**__",
                    Description = description,
                    Timestamp = DateTime.UtcNow
                };

                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch
            {
                if (trackType != "rts" && trackType != "cts")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track type is not valid.*" +
                        "\n**c!wwpop [rts/cts] [range(1-32/218)]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Unknown Error.*" +
                        "\n**c!wwpop [rts/cts] [range(1-32/218)]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("ttpop")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task TTPopularityRequest(CommandContext ctx, string trackType = "rts", int display = 0)
        {
            var embed = new DiscordEmbedBuilder { };

            string json = "";

            if (trackType == "rts")
            {
                if (display < 1)
                {
                    display = 1;
                }
                else if (display > 12)
                {
                    display = 12;
                }
            }
            else if (trackType == "cts")
            {
                if (display < 1)
                {
                    display = 1;
                }
                else if (display > 198)
                {
                    display = 198;
                }
            }

            try
            {
                json = File.ReadAllText($"{trackType}.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);
                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Category % 16 != 0)
                    {
                        trackList.RemoveAt(i);
                        i--;
                    }
                }
                trackList = trackList.OrderBy(a => a.TimeTrialScore).ToList();
                trackList.Reverse();

                string description = "";

                for (int i = display - 1; i < display + 20; i++)
                {
                    description = description + $"**{i + 1})** {trackList[i].Name} *({trackList[i].TimeTrialScore})*\n";
                }

                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Displaying {display} - {display + 20}:**__",
                    Description = description,
                    Timestamp = DateTime.UtcNow
                };

                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch
            {
                if (trackType != "rts" && trackType != "cts")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track type is not valid.*" +
                        "\n**c!wwpop [rts/cts] [range(1-32/218)]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Unknown Error.*" +
                        "\n**c!wwpop [rts/cts] [range(1-32/218)]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("wwpopsearch")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task WWPopularitySearch(CommandContext ctx, string trackType = "rts", [RemainingText] string track = "")
        {
            var embed = new DiscordEmbedBuilder { };

            string json = "";

            try
            {
                json = File.ReadAllText($"{trackType}.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);
                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Category % 16 != 0)
                    {
                        trackList.RemoveAt(i);
                        i--;
                    }
                }
                trackList = trackList.OrderBy(a => a.WiimmfiScore).ToList();
                trackList.Reverse();

                string description = "";

                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Name.ToLowerInvariant().Contains(track.ToLowerInvariant()))
                    {
                        description = description + $"**{i + 1})** {trackList[i].Name} *({trackList[i].WiimmfiScore})*\n";
                    }
                }

                if (description == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*" +
                        "\n**c!wwpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }

                else if (track.Length < 3)
                {

                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Please input a track name to filter with (min 3 chars).*" +
                           "\n**c!wwpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }

                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying tracks containing *{track}*:**__",
                        Description = description,
                        Timestamp = DateTime.UtcNow
                    };
                }

                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch
            {
                if (trackType != "rts" && trackType != "cts")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track type is not valid.*" +
                        "\n**c!wwpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else if (track == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track argument is missing.*" +
                        "\n**c!wwpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Unknown Error.*" +
                        "\n**c!wwpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("ttpopsearch")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task TTPopularitySearch(CommandContext ctx, string trackType = "rts", [RemainingText] string track = "")
        {
            var embed = new DiscordEmbedBuilder { };

            string json = "";

            try
            {
                json = File.ReadAllText($"{trackType}.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);
                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Category % 16 != 0)
                    {
                        trackList.RemoveAt(i);
                        i--;
                    }
                }
                trackList = trackList.OrderBy(a => a.TimeTrialScore).ToList();
                trackList.Reverse();

                string description = "";

                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Name.ToLowerInvariant().Contains(track.ToLowerInvariant()))
                    {
                        description = description + $"**{i + 1})** {trackList[i].Name} *({trackList[i].TimeTrialScore})*\n";
                    }
                }

                if (description == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*" +
                        "\n**c!ttpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }

                else if (track.Length < 3)
                {

                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Please input a track name to filter with (min 3 chars).*" +
                           "\n**c!ttpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }

                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying tracks containing *{track}*:**__",
                        Description = description,
                        Timestamp = DateTime.UtcNow
                    };
                }

                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch
            {
                if (trackType != "rts" && trackType != "cts")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track type is not valid.*" +
                        "\n**c!ttpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else if (track == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track argument is missing.*" +
                        "\n**c!ttpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Unknown Error.*" +
                        "\n**c!ttpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }
    }
}