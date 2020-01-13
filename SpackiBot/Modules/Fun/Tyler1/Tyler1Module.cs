using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpackiBot.Modules.Fun.Tyler1
{
    [Name("Tyler")]
    [Group("tyler")]
    [Alias(new string[] { "t", "deformed" })]
    public class Tyler1Module : ModuleBase<SocketCommandContext>
    {
        [Name("tyler")]
        [Command()]
        [Summary("Tyler motherfucking one")]
        public async Task Tyler()
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync("Tyler motherfucking one");
        }

        [Command("play")]
        [Summary("Spielt einen Tyler1-Sound in deinem Voice-Channel")]
        public async Task PlayAsync([Summary("(Optional) Filter für bestimmten Sound")] string filter = null)
        {
            await Task.Delay(1);
        }

        [Command("quote")]
        [Summary("Gibt ein Tyler1-Zitat aus")]
        public async Task QuoteAsync([Summary("(Optional) Filter für bestimmtes Zitat")] string filter = null)
        {
            await ReplyAsync("Test");
        }
    }
}