using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using Discord.WebSocket;
using SpackiBot.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SpackiBot.Modules.Help
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private CommandService _commandService;

        public HelpModule(CommandService commandService)
        {
            _commandService = commandService;
        }

        [Command("help")]
        [Alias(new string[] { "h", "how", "?" })]
        [Summary("Shows available commands and their description")]
        public async Task HelpAsync()
        {
            try
            {
                var builder = new EmbedBuilder()
                    .WithColor(new Color(255, 0, 255))
                    .WithAuthor(Context.Client.CurrentUser)
                    .WithTitle("Befehle")
                    .WithDescription("Auflistung der Befehle von SpackiBot:")
                    .WithFooter(footer => footer.Text = "You have no mana!")
                    .WithCurrentTimestamp();

                foreach (var module in _commandService.Modules)
                {
                    StringBuilder descriptionBuilder = new StringBuilder();
                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess)
                        {
                            descriptionBuilder.AppendLine($"{cmd.Name} [{string.Join(", ", cmd.Aliases.Where(alias => Array.IndexOf(cmd.Aliases.ToArray(), alias) != 0))}]");
                            descriptionBuilder.AppendLine(cmd.Summary);
                            descriptionBuilder.AppendLine();
                        }
                    }

                    string description = descriptionBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        builder.AddField(field =>
                        {
                            field.Name = module.Name;
                            field.Value = description;
                            field.IsInline = false;
                        });
                    }
                }

                await ReplyAsync("", false, builder.Build());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace + Environment.NewLine + e.Message);
            }
        }
    }
}