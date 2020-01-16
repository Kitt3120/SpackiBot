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
using Microsoft.VisualBasic;
using System.Collections.Immutable;
using System.Security.Cryptography;

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

        [Command("Help")]
        [Alias(new string[] { "h", "how", "?" })]
        [Summary("Zeigt verfügbare Befehle mit ihren Parametern und Beschreibungen")]
        public async Task HelpAsync()
        {
            try
            {
                var builder = new EmbedBuilder()
                    .WithColor(new Color(255, 0, 255))
                    .WithAuthor(Context.Client.CurrentUser)
                    .WithTitle("Befehle")
                    .WithDescription("Auflistung der Befehle von SpackiBot:")
                    .WithFooter(footer => footer.Text = "YOU HAVE NO MANA!")
                    .WithCurrentTimestamp();

                foreach (var module in _commandService.Modules.OrderBy(module => module.Name))
                {
                    StringBuilder descriptionBuilder = new StringBuilder();
                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess)
                        {
                            descriptionBuilder.AppendLine($"{cmd.Name}:");
                            for (int i = 0; i < cmd.Parameters.Count; i++)
                                descriptionBuilder.AppendLine($"> Parameter {i + 1}: {cmd.Parameters[i].Name} - {cmd.Parameters[i].Summary}");
                            descriptionBuilder.AppendLine($"> {cmd.Summary}");
                            descriptionBuilder.AppendLine();
                        }
                    }

                    string description = descriptionBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        builder.AddField(field =>
                        {
                            field.Name = $"Module {module.Name}{(module.Aliases == null || module.Aliases.Count == 1 ? "" : " [" + string.Join(", ", module.Aliases.Where(alias => Array.IndexOf(module.Aliases.ToArray(), alias) != 0)) + "]")}";
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