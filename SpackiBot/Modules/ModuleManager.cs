using Discord.Commands;
using Discord.WebSocket;
using SpackiBot.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpackiBot.Modules
{
    class ModuleManager
    {
        private LoggingSection _localSection;

        private SpackiBot _spackiBot;
        private CommandService _commandService;

        public ModuleManager(SpackiBot spackiBot, CommandService commandService)
        {
            _localSection = new LoggingSection("Module-Manager");
            _localSection.Verbose("Reached Module-Manager");

            _spackiBot = spackiBot;
            _commandService = commandService;

            InstallCommandsAsync().GetAwaiter().GetResult();
        }

        //From https://docs.stillu.cc/guides/commands/intro.html
        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _spackiBot.Discord.MessageReceived += HandleCommandAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await _commandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _spackiBot.ServiceProvider);
        }

        //From https://docs.stillu.cc/guides/commands/intro.html
        private async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            // Don't process the command if it was a system message
            var message = socketMessage as SocketUserMessage;
            if (message == null)
                return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(_spackiBot.Discord.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_spackiBot.Discord, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commandService.ExecuteAsync(context, argPos, _spackiBot.ServiceProvider);
        }
    }
}
