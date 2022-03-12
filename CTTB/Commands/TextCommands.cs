﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTTB.Commands
{
    public class TextCommands : BaseCommandModule
    {
        [Command("help")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task Help(CommandContext ctx)
        {
            string description = "__**Standard Commands:**__" +
                "\nc!help" +
                "\nc!cttp" +
                "\nc!source" +
                "\nc!staff track" +
                "\nc!issues" +
                "\nc!getissues track" +
                "\nc!getinfo track" +
                "\nc!bkt track" +
                "\nc!pop rts/cts/track" +
                "\nc!ttpop rts/cts/track" +
                "\nc!getsummary track" +
                "\nc!nextupdate";

            foreach (var role in ctx.Member.Roles)
            {
                if (role.Name == "Track Council")
                {
                    description += "\n\n__**Council Commands:**__" +
                        "\nc!hw" +
                        "\nc!gethw track" +
                        "\nc!submithw yes/fixes/neutral/no \"track\" feedback";
                }
                if (role.Name == "Pack & Bot Dev" || role.Name == "Admin")
                {
                    description += "\n\n__**Admin Commands:**__" +
                        "\nc!update" +
                        "\nc!lastupdated" +
                        "\nc!reportissue major/minor \"track\" -Issue" +
                        "\nc!clearissues track" +
                        "\nc!replaceissues \"old track\" \"new track\" \"author\" \"version\" \"slot\" laps" +
                        "\nc!gethw \"track\" mention/name" +
                        "\nc!addhw \"track\" \"author\" \"version\" \"download link\" \"slot-filename\" \"speed/lap modifiers\" notes" +
                        "\nc!delhw track";
                }
            }

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = "__**Help**__",
                Description = description,
                Timestamp = DateTime.UtcNow
            };

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("cttp")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task CTTP(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = "__**Custom Track Test Pack Links**__",
                Description = "**Base Pack:** *https://drive.google.com/file/d/1tzhaBxycHEvY5G2k5jTzpONBhmq6g08g/view?usp=sharing*" +
                "\n**Installation Tutorial:** *https://youtu.be/Vh3GXTbxHLU*" +
                "\n**Test Pack Creation Tutorial:** *https://youtu.be/igtfmZAyG3g*",
                Timestamp = DateTime.UtcNow
            };

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("source")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task SourceCode(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = "__**Source Code:**__",
                Description = "**Github:** *https://github.com/Brawlboxgaming/Custom-Track-Testing-Bot*",
                Timestamp = DateTime.UtcNow
            };

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }
    }
}
