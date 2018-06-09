using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KronixGamer1.Core.UserAccounts;
using NReco.ImageGenerator;
using System.Net;
using Newtonsoft.Json;
using Discord.Rest;

namespace KronixGamer1.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("Warn")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task WarnUser(IGuildUser user)
        {
            var userAccount = UserAccounts.GetAccount((SocketUser)user);
            userAccount.NumberOfWarnings++;
            UserAccounts.SaveAccounts();

            if(userAccount.NumberOfWarnings >= 3)
            {
                await user.Guild.AddBanAsync(user, 5);
            }
             else if(userAccount.NumberOfWarnings == 2)
            {
                // perhaps kick
            }
            else if (userAccount.NumberOfWarnings == 1)
            {
                // perhaps send warning message
            }
        }

        [Command("Kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickUser(IGuildUser user, string reason = "No reason provided.")
        {
            await user.KickAsync(reason);
        }

        [Command("Ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUser(IGuildUser user, string reason = "No reason provided.")
        {
            await user.Guild.AddBanAsync(user, 5, reason);
        }

        [Command("KickUseroffline")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task Test(int days)
        {
            var guild = Context.Guild;
            await guild.PruneUsersAsync(days);
            await ReplyAsync($"Kicked everyone who was inactive for more than {0} days!");
        }

        [Command("WhatLevelIs")]
        public async Task WhatLevelIs(uint xp)
        {
            uint level = (uint)Math.Sqrt(xp / 50);
            await Context.Channel.SendMessageAsync("The level is " + level);
        }

        [Command("react")]
        public async Task HandleReactionMessage()
        {
            RestUserMessage msg = await Context.Channel.SendMessageAsync("React to me!");
            Global.MessageIdToTrack = msg.Id;
        }

        [Command("person")]
        public async Task GetRandomPerson()
        {
            string json = "";
            using (WebClient client = new WebClient())
            {
                json = client.DownloadString("https://randomuser.me/api/?gender=female&nat=US");
            }

            var dataObject = JsonConvert.DeserializeObject<dynamic>(json);

            string firstName = dataObject.results[0].name.first.ToString();
            string lastName = dataObject.results[0].name.last.ToString();
            string avatarURL = dataObject.results[0].picture.large.ToString();

            var embed = new EmbedBuilder();
            embed.WithThumbnailUrl(avatarURL);
            embed.WithTitle("Generated Person");
            embed.AddInlineField("First Name", firstName);
            embed.AddInlineField("Last Name", lastName);

            await Context.Channel.SendMessageAsync("", embed: embed);
        }

        [Command("hello")]
        public async Task Hello(string color = "#c30d0d")
        {
            string css = "<style>\n    hi{\n        coler: #c30d0d;\n    }\n</style>\n";
            string html = String.Format("<h1>HELLO {0}!</h1>", Context.User.Username);
            var converter = new HtmlToImageConverter
            {
                   Width = 50,
                   Height = 50
            };
            var jpgBytes = converter.GenerateImage(css + html, NReco.ImageGenerator.ImageFormat.Jpeg);
            await Context.Channel.SendFileAsync(new MemoryStream(jpgBytes), "HELLO.jpg");
        }

        [Command("myStats")]
        public async Task  MyStats([Remainder]string arg = "")
        {
            SocketUser target = null;
            var mentionedUser = Context.Message.MentionedUsers.FirstOrDefault();
            target = mentionedUser ?? Context.User;

            var account = UserAccounts.GetAccount(target);
            await Context.Channel.SendMessageAsync($"{target.Username} has {account.XP} XP and {account.Points} points.");
        }

        [Command("addXP")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddXP(uint xp)
        {
            var account =  UserAccounts.GetAccount(Context.User);
            account.XP += xp;
            UserAccounts.SaveAccounts();
            await Context.Channel.SendMessageAsync($"You gained {xp} XP.");
        }

        [Command("echo")]
        public async Task Echo([Remainder]string message)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Message by " + Context.User.Username);
            embed.WithDescription(message);
            embed.WithColor(new Color(0, 255, 0));
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("pick")]
        public async Task PickOne([Remainder]string message)
        {
            string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            Random r = new Random();
            string selection = options[r.Next(0, options.Length)];

            var embed = new EmbedBuilder();
            embed.WithTitle("Choice for " + Context.User.Username);
            embed.WithDescription(selection);
            embed.WithColor(new Color(255, 255, 0));
            embed.WithThumbnailUrl("https://orig00.deviantart.net/3033/f/2016/103/0/c/mercy_by_raichiyo33-d9yufl4.jpg");

            await Context.Channel.SendMessageAsync("", false, embed);
             DataStorage.AddPairToStorage(Context.User.Username + DateTime.Now.ToLongDateString(), selection);
        }

        [Command("Help")]
        public void Help()
        {
            var embed = new EmbedBuilder();

            embed.WithTitle("URL");

            embed.WithDescription("URL: URL.com");

            embed.WithFooter("CoolText");
        }

        [Command("data")]
        public async Task GetData()
        {
            await Context.Channel.SendMessageAsync("Data Has " + DataStorage.GetPairsCount() + " pairs.");
        }
    }
}
