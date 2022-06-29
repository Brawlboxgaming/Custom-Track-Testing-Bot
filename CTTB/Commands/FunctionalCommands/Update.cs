﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using IronPython.Runtime.Operations;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTTB.Commands
{
    public class Update : BaseCommandModule
    {
        public Util Utility = new Util();

        [Command("starttimer")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task StartTimers(CommandContext ctx, [RemainingText] string arg = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                await ctx.TriggerTypingAsync();
                var embed = new DiscordEmbedBuilder() { };

                if (Utility.dailyTimer != null)
                {
                    Utility.dailyTimer.Stop();
                }
                Utility.dailyTimer = new System.Timers.Timer(86400000);
                Utility.dailyTimer.AutoReset = true;
                Utility.dailyTimer.Elapsed += async (s, e) => await UpdateJsons(ctx, "all");
                Utility.dailyTimer.Elapsed += async (s, e) => await CheckHw(ctx, "");
                Utility.dailyTimer.Start();
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Notice:**__",
                    Description = "Timer has been started.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("update")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task UpdateInit(CommandContext ctx, [RemainingText] string arg = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                await ctx.TriggerTypingAsync();
                await UpdateJsons(ctx, arg);

                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Notice:**__",
                    Description = "Database has been updated.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };

                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        public async Task UpdateJsons(CommandContext ctx, string arg)
        {
            try
            {
                string rtttUrl = "http://tt.chadsoft.co.uk/original-track-leaderboards.json";
                string ctttUrl = "http://tt.chadsoft.co.uk/ctgp-leaderboards.json";
                string rttt200Url = "http://tt.chadsoft.co.uk/original-track-leaderboards-200cc.json";
                string cttt200Url = "http://tt.chadsoft.co.uk/ctgp-leaderboards-200cc.json";
                string ctwwUrl1 = "https://wiimmfi.de/stats/track/mv/ctgp?m=json&p=std,c1,0";
                string ctwwUrl2 = "https://wiimmfi.de/stats/track/mv/ctgp?m=json&p=std,c1,0,100";
                string ctwwUrl3 = "https://wiimmfi.de/stats/track/mv/ctgp?m=json&p=std,c1,0,200";
                string wwUrl = "https://wiimmfi.de/stats/track/mv/ww?m=json&p=std,c1,0";

                // Leaderboards

                var webClient = new WebClient();

                var rtRawJson = JsonConvert.DeserializeObject<LeaderboardInfo>(await webClient.DownloadStringTaskAsync(rtttUrl));
                var ctRawJson = JsonConvert.DeserializeObject<LeaderboardInfo>(await webClient.DownloadStringTaskAsync(ctttUrl));
                var rtRaw200Json = JsonConvert.DeserializeObject<LeaderboardInfo>(await webClient.DownloadStringTaskAsync(rttt200Url));
                var ctRaw200Json = JsonConvert.DeserializeObject<LeaderboardInfo>(await webClient.DownloadStringTaskAsync(cttt200Url));

                foreach (var track in rtRawJson.Leaderboard)
                {
                    track.LeaderboardLink = track.Link.Href.LeaderboardLink;
                    track.Link = null;
                }
                foreach (var track in ctRawJson.Leaderboard)
                {
                    track.LeaderboardLink = track.Link.Href.LeaderboardLink;
                    track.Link = null;
                }
                foreach (var track in rtRaw200Json.Leaderboard)
                {
                    track.LeaderboardLink = track.Link.Href.LeaderboardLink;
                    track.Link = null;
                }
                foreach (var track in ctRaw200Json.Leaderboard)
                {
                    track.LeaderboardLink = track.Link.Href.LeaderboardLink;
                    track.Link = null;
                }

                var rtJson = JsonConvert.SerializeObject(rtRawJson.Leaderboard);
                var ctJson = JsonConvert.SerializeObject(ctRawJson.Leaderboard);
                var rt200Json = JsonConvert.SerializeObject(rtRaw200Json.Leaderboard);
                var ct200Json = JsonConvert.SerializeObject(ctRaw200Json.Leaderboard);

                string ctwwDl1 = await webClient.DownloadStringTaskAsync(ctwwUrl1);
                string ctwwDl2 = await webClient.DownloadStringTaskAsync(ctwwUrl2);
                string ctwwDl3 = await webClient.DownloadStringTaskAsync(ctwwUrl3);
                string wwDl = await webClient.DownloadStringTaskAsync(wwUrl);

                List<Track> trackListRt = JsonConvert.DeserializeObject<List<Track>>(rtJson);
                List<Track> trackListRt200 = JsonConvert.DeserializeObject<List<Track>>(rt200Json);
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(ctJson);
                List<Track> trackList200 = JsonConvert.DeserializeObject<List<Track>>(ct200Json);
                List<Track> trackListNc = JsonConvert.DeserializeObject<List<Track>>(ctJson);
                List<Track> trackList200Nc = JsonConvert.DeserializeObject<List<Track>>(ct200Json);
                for (int i = 0; i < trackListNc.Count; i++)
                {
                    if (trackListNc[i].Category % 16 != 0)
                    {
                        trackListNc.RemoveAt(i);
                        i--;
                    }
                }
                for (int i = 0; i < trackList200Nc.Count; i++)
                {
                    if (trackList200Nc[i].Category % 16 != 0 || trackList200Nc[i].Category != 4)
                    {
                        trackList200Nc.RemoveAt(i);
                        i--;
                    }
                }

                string oldRtJson = File.ReadAllText("rts.json");
                List<Track> oldRtTrackList = JsonConvert.DeserializeObject<List<Track>>(oldRtJson);

                for (int i = 0; i < oldRtTrackList.Count; i++)
                {
                    trackListRt[i].WiimmfiName = oldRtTrackList[i].WiimmfiName;
                    trackListRt[i].WiimmfiScore = oldRtTrackList[i].WiimmfiScore;
                    trackListRt[i].BKTLink = oldRtTrackList[i].BKTLink;
                    trackListRt[i].BKTHolder = oldRtTrackList[i].BKTHolder;
                    trackListRt[i].CategoryName = oldRtTrackList[i].CategoryName;
                }

                oldRtJson = File.ReadAllText("rts200.json");
                oldRtTrackList = JsonConvert.DeserializeObject<List<Track>>(oldRtJson);

                for (int i = 0; i < oldRtTrackList.Count; i++)
                {
                    trackListRt200[i].WiimmfiName = oldRtTrackList[i].WiimmfiName;
                    trackListRt200[i].WiimmfiScore = oldRtTrackList[i].WiimmfiScore;
                    trackListRt200[i].BKTLink = oldRtTrackList[i].BKTLink;
                    trackListRt200[i].BKTHolder = oldRtTrackList[i].BKTHolder;
                    trackListRt200[i].CategoryName = oldRtTrackList[i].CategoryName;
                }

                string oldJson = File.ReadAllText("cts.json");
                List<Track> oldTrackList = JsonConvert.DeserializeObject<List<Track>>(oldJson);

                for (int i = 0; i < oldTrackList.Count; i++)
                {
                    trackList[i].WiimmfiName = oldTrackList[i].WiimmfiName;
                    trackList[i].WiimmfiScore = oldTrackList[i].WiimmfiScore;
                    trackList[i].BKTLink = oldTrackList[i].BKTLink;
                    trackList[i].BKTHolder = oldTrackList[i].BKTHolder;
                    trackList[i].CategoryName = oldTrackList[i].CategoryName;
                }

                oldJson = File.ReadAllText("cts200.json");
                oldTrackList = JsonConvert.DeserializeObject<List<Track>>(oldJson);

                for (int i = 0; i < oldTrackList.Count; i++)
                {
                    trackList200[i].WiimmfiName = oldTrackList[i].WiimmfiName;
                    trackList200[i].WiimmfiScore = oldTrackList[i].WiimmfiScore;
                    trackList200[i].BKTLink = oldTrackList[i].BKTLink;
                    trackList200[i].BKTHolder = oldTrackList[i].BKTHolder;
                    trackList200[i].CategoryName = oldTrackList[i].CategoryName;
                }

                try
                {
                    if (Utility.CompareIncompleteStrings(arg, "wiimmfi") || Utility.CompareIncompleteStrings(arg, "all"))
                    {
                        await Utility.Scraper.WiimmfiScrape(rtJson,
                            rt200Json,
                            ctwwDl1,
                            ctwwDl2,
                            ctwwDl3,
                            wwDl,
                            trackListRt,
                            trackListRt200,
                            trackList,
                            trackList200,
                            trackListNc,
                            trackList200Nc);
                    }
                }
                catch
                {
                    Thread.Sleep(300000);
                    await Utility.Scraper.WiimmfiScrape(rtJson,
                             rt200Json,
                             ctwwDl1,
                             ctwwDl2,
                             ctwwDl3,
                             wwDl,
                             trackListRt,
                             trackListRt200,
                             trackList,
                             trackList200,
                             trackListNc,
                             trackList200Nc);
                }

                try
                {
                    if (Utility.CompareIncompleteStrings(arg, "bkts") || Utility.CompareIncompleteStrings(arg, "all"))
                    {
                        await Utility.Scraper.GetBKTLeaderboards(trackListRt, trackListRt200, trackList, trackList200);
                    }
                }
                catch
                {
                    Thread.Sleep(300000);
                    await Utility.Scraper.GetBKTLeaderboards(trackListRt, trackListRt200, trackList, trackList200);
                }

                try
                {
                    await Utility.Scraper.GetSlotIds(trackListRt, trackListRt200, trackList, trackList200);
                }
                catch
                {
                    Thread.Sleep(300000);
                    await Utility.Scraper.GetSlotIds(trackListRt, trackListRt200, trackList, trackList200);
                }

                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                ctJson = JsonConvert.SerializeObject(trackList, settings);
                ct200Json = JsonConvert.SerializeObject(trackList200, settings);

                rtJson = JsonConvert.SerializeObject(trackListRt, settings);
                rt200Json = JsonConvert.SerializeObject(trackListRt200, settings);

                File.WriteAllText("rts.json", rtJson);
                File.WriteAllText("cts.json", ctJson);
                File.WriteAllText("rts200.json", rt200Json);
                File.WriteAllText("cts200.json", ct200Json);

                var today = DateTime.Now;
                File.WriteAllText("lastUpdated.txt", today.ToString());

                var processInfo = new ProcessStartInfo();
                processInfo.FileName = @"sudo";
                processInfo.Arguments = $"cp rts.json /var/www/brawlbox/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                var process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"sudo";
                processInfo.Arguments = $"cp rts200.json /var/www/brawlbox/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"sudo";
                processInfo.Arguments = $"cp cts.json /var/www/brawlbox/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"sudo";
                processInfo.Arguments = $"cp cts200.json /var/www/brawlbox/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();
            }

            catch (Exception ex)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                              "\n**c!update**",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("checkmissedhw")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task CheckHwInit(CommandContext ctx, [RemainingText] string arg = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                await ctx.TriggerTypingAsync();
                await CheckHw(ctx, arg);

                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Notice:**__",
                    Description = $"*Any missed homework has been recorded.*",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };

                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        public async Task CheckHw(CommandContext ctx, [RemainingText] string placeholder)
        {
            await ctx.TriggerTypingAsync();
            var embed = new DiscordEmbedBuilder() { };
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                {
                    List<string> trackDisplay = new List<string>();
                    string json;

                    string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";
                    var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);
                    ServiceAccountCredential credential = new ServiceAccountCredential(
                       new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));
                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Custom Track Testing Bot",
                    });

                    var temp = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluation Log'");
                    var tempResponse = await temp.ExecuteAsync();
                    var today = int.Parse(tempResponse.Values[tempResponse.Values.Count - 1][tempResponse.Values[tempResponse.Values.Count - 1].Count - 1].ToString());

                    var request = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                    request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                    var response = await request.ExecuteAsync();
                    foreach (var t in response.Values)
                    {
                        while (t.Count < response.Values[0].Count)
                        {
                            t.Add("");
                        }
                    }

                    using (var fs = File.OpenRead("council.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    for (int i = 1; i < response.Values.Count; i++)
                    {
                        if (today == int.Parse(response.Values[i][1].ToString()))
                        {
                            trackDisplay.Add(response.Values[i][0].ToString());
                        }
                        if (Utility.lastHwDateChecked != int.Parse(response.Values[i][1].ToString()) && today > int.Parse(response.Values[i][1].ToString()))
                        {
                            Utility.lastHwDateChecked = int.Parse(response.Values[i][1].ToString());
                            for (int j = 12; j < response.Values[0].Count; j++)
                            {
                                bool check = false;
                                for (int k = 1; k < response.Values.Count; k++)
                                {
                                    if ((response.Values[k][j].ToString() == "" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "yes" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "no" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "neutral" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "fixes" ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("yes") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("no") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("neutral") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("fixes")) &&
                                        response.Values[k][j].ToString() != "This member is the author thus cannot vote")
                                    {
                                        check = true;
                                    }
                                }
                                int ix = councilJson.FindIndex(x => x.SheetName == response.Values[0][j].ToString());
                                if (check)
                                {
                                    councilJson[ix].TimesMissedHw++;
                                    councilJson[ix].HwInARow = 0;
                                    if (councilJson[ix].TimesMissedHw > 0 && councilJson[ix].TimesMissedHw % 3 == 0)
                                    {
                                        string message = $"Hello {councilJson[ix].SheetName}. Just to let you know, you appear to have not completed council homework in a while, have been inconsistent with your homework, or are not completing it sufficiently enough. Just to remind you, if you miss homework too many times, admins might have to remove you from council. If you have an issue which stops you from doing homework, please let an admin know.";

                                        var members = ctx.Guild.GetAllMembersAsync();
                                        foreach (var member in members.Result)
                                        {
                                            if (member.Id == councilJson[ix].DiscordId)
                                            {
                                                try
                                                {
                                                    Console.WriteLine($"DM'd Member: {councilJson[ix].SheetName}");
                                                    await member.SendMessageAsync(message).ConfigureAwait(false);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine(ex.Message);
                                                    Console.WriteLine("DMs are likely closed.");
                                                }
                                            }
                                        }
                                    }
                                }
                                bool completed = true;
                                for (int k = 1; k < response.Values.Count; k++)
                                {
                                    if ((response.Values[k][j].ToString() == "" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "yes" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "no" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "neutral" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "fixes" ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("yes") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("no") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("neutral") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("fixes")) &&
                                        response.Values[k][j].ToString() != "This member is the author thus cannot vote")
                                    {
                                        completed = false;
                                    }
                                }
                                if (completed)
                                {
                                    councilJson[ix].HwInARow++;
                                    if (councilJson[ix].HwInARow > 4)
                                    {
                                        councilJson[ix].TimesMissedHw = 0;
                                    }
                                }
                            }
                        }
                    }
                    string council = JsonConvert.SerializeObject(councilJson);
                    File.WriteAllText("council.json", council);

                    DiscordChannel channel = ctx.Channel;

                    foreach (var c in ctx.Guild.Channels)
                    {
                        if (c.Value.Id == 635313521487511554)
                        {
                            channel = c.Value;
                        }
                    }

                    string listOfTracks = "";
                    if (trackDisplay.Count > 0)
                    {
                        listOfTracks = trackDisplay[0];
                        for (int i = 1; i < trackDisplay.Count - 1; i++)
                        {
                            listOfTracks += $", {trackDisplay[i]}";
                        }
                        if (trackDisplay.Count == 1)
                        {
                            listOfTracks += " is";
                        }
                        else
                        {
                            listOfTracks += " are";
                        }

                        await channel.SendMessageAsync($"<@&608386209655554058> {listOfTracks} due for today.");
                    }
                }
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                       "\n**c!checkmissedhw**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }
    }
}