using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SpackiBot.Logging;
using SpackiBot.Modules;
using SpackiBot.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpackiBot
{
    internal class SpackiBot
    {
        private LoggingSection _localSection;
        private LoggingSection _discordSection;

        public IServiceProvider ServiceProvider { get; private set; }
        public DiscordSocketClient Discord { get; private set; }
        private bool _running = false;

        public static string DiscordToken { get; private set; } = "Nope";

        public SpackiBot()
        {
            _localSection = new LoggingSection("SpackiBot");
            _discordSection = new LoggingSection("Discord");

            _localSection.Verbose("Reached init");
            _localSection.Info("Starting SpackiBot");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Discord = new DiscordSocketClient();
            SetupDiscord().GetAwaiter().GetResult();
            SetupServices();

            sw.Stop();
            _localSection.Info($"SpackiBot started, took {sw.ElapsedMilliseconds}ms to start.");

            //Keep main thread alive so program doesn't close
            _localSection.Debug("Keeping program alive");
            while (_running)
                Thread.Sleep(5000);
        }

        private async Task SetupDiscord()
        {
            using (LoggingSection section = _localSection.CreateChild("Discord-Setup"))
            {
                section.Verbose("Reached Discord-Setup");

                section.Debug("Registering events");
                bool ready = false;
                Discord.Ready += () => { ready = true; return Task.CompletedTask; };
                Discord.Log += LogDiscordAsync;

                section.Debug("Logging in...");
                await Discord.LoginAsync(TokenType.Bot, DiscordToken);
                section.Debug("Starting bot...");
                await Discord.StartAsync();
                while (!ready)
                    await Task.Delay(100);
                await Task.Delay(5000); //After the bot started, even after Ready-Event has been fired, it still needs some preparation time. This is a known "bug" and Task.Delay(5000); is a nice work-around.

                _running = true;
            }
        }

        private void SetupServices()
        {
            using (LoggingSection section = _localSection.CreateChild("Service-Setup"))
            {
                section.Verbose("Reached Service-Setup");

                IServiceCollection serviceCollection = new ServiceCollection();

                section.Debug("Adding SpackiBot-Instance to ServiceCollection");
                serviceCollection.AddSingleton(this);

                section.Debug("Adding AssetService to ServiceCollection");
                serviceCollection.AddSingleton(new AssetService());

                section.Debug("Adding DiscordSocketClient to ServiceCollection");
                serviceCollection.AddSingleton(Discord);

                section.Debug("Adding CommandService to ServiceCollection");
                CommandService commandService = new CommandService(new CommandServiceConfig()
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async
                });
                serviceCollection.AddSingleton(commandService);

                section.Debug("Creating ModuleManager");
                serviceCollection.AddSingleton(new ModuleManager(this, commandService));

                section.Debug("Building ServiceProvider");
                ServiceProvider = serviceCollection.BuildServiceProvider();
            }
        }

        public async Task Shutdown()
        {
            _localSection.Debug("Reached shutdown");
            _localSection.Info("Shutting down...");
            await Discord.LogoutAsync();
            Discord.Dispose();
            _localSection.Dispose();
            _running = false;
        }

        private Task LogDiscordAsync(Discord.LogMessage logMessage)
        {
            _discordSection.Log(Enum.Parse<LogLevel>(logMessage.Severity.ToString()), logMessage.Message);
            return Task.CompletedTask;
        }
    }
}