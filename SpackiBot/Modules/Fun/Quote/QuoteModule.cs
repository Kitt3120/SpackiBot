using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace SpackiBot.Modules.Fun.Quote
{
    [Name("Quote")]
    public class QuoteModule : ModuleBase<SocketCommandContext>
    {
        private static Random _colorRandom = new Random();

        [Command("Quote")]
        [Summary("Erstellt ein Zitat und postet es im Quote-Channel")]
        [Alias(new string[] { "q" })]
        public async Task QuoteAsync([Summary("Autor des Zitats")] SocketUser user = null, [Remainder] [Summary("Text des Zitats")] string text = null)
        {
            if (user == null || text == null)
            {
                await ReplyAsync("Du musst sowohl User als auch Text angeben!");
                return;
            }

            var builder = new EmbedBuilder()
                .WithAuthor(user)
                .WithTitle(text)
                .WithColor(new Color(_colorRandom.Next(255), _colorRandom.Next(255), _colorRandom.Next(255)))
                .WithFooter(new EmbedFooterBuilder().WithText($"Zitat erstellt von {Context.User.Username}#{Context.User.Discriminator}"))
                .WithTimestamp(DateTime.Now);

            await ReplyAsync("", false, builder.Build());
        }
    }
}