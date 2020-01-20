using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SpackiBot.ConsoleCommands;
using SpackiBot.Logging;
using SpackiBot.Modules;
using SpackiBot.Properties;
using SpackiBot.Services.AssetService;
using SpackiBot.Services.FFmpeg;
using SpackiBot.Services.VoiceService;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SpackiBot
{
    public class SpackiBot
    {
        private LoggingSection _localSection;
        private LoggingSection _discordSection;

        public IServiceProvider ServiceProvider { get; private set; }

        private DiscordSocketClient _discord;
        private ConsoleCommandManager _consoleCommandManager;
        private bool _running = false;

        //TODO: fix Tyler play on invalid command parameters

        public SpackiBot()
        {
            _localSection = new LoggingSection("SpackiBot");
            _discordSection = new LoggingSection("Discord");

            _localSection.Verbose("Reached init");
            _localSection.Info("Starting SpackiBot");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            CheckSettings();
            try
            {
                SetupDiscord().GetAwaiter().GetResult();
            }
            catch (HttpException)
            {
                Tokens.Default.DiscordBot = null;
                Tokens.Default.Save();
                Console.WriteLine("Seems like your Discord Bot-Token is not valid. Please provide a new token on next start.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            SetupServices();
            _consoleCommandManager = new ConsoleCommandManager(ServiceProvider);
            _running = true;

            sw.Stop();
            _localSection.Info($"SpackiBot started, took {sw.ElapsedMilliseconds}ms to start.");

            //Also keeps main thread alive so program doesn't close
            while (_running)
            {
                _consoleCommandManager.Handle(Console.ReadLine());
                Thread.Sleep(1000); //Otherwise sometimes it will still be stuck in this loop as _running is still true
            }
        }

        private void CheckSettings()
        {
            using (LoggingSection section = _localSection.CreateChild("Settings"))
            {
                section.Verbose("Reached Settings-Check");

                section.Verbose("Checking for Discord Bot-Token...");
                if (string.IsNullOrWhiteSpace(Tokens.Default.DiscordBot))
                {
                    Console.WriteLine("Please provide a Discord Bot-Token: ");
                    Tokens.Default.DiscordBot = Console.ReadLine();
                    Tokens.Default.Save();
                    section.Info("Discord Bot-Token saved");
                }
                else
                    section.Verbose("Discord Bot-Token is provided");

                section.Verbose("Settings checked");
            }
        }

        //This does not check if the settings are valid!
        private async Task SetupDiscord()
        {
            using (LoggingSection section = _localSection.CreateChild("Discord-Setup"))
            {
                section.Verbose("Reached Discord-Setup");

                bool ready = false;
                _discord = new DiscordSocketClient();

                section.Verbose("Registering events");
                _discord.Ready += () => { ready = true; return Task.CompletedTask; };
                _discord.Log += LogDiscordAsync;

                section.Verbose("Logging in...");
                await _discord.LoginAsync(TokenType.Bot, Tokens.Default.DiscordBot);
                section.Verbose("Starting bot...");
                await _discord.StartAsync();

                while (!ready)
                    await Task.Delay(100);
                //await Task.Delay(1000); //After the bot started, even after Ready-Event has been fired, it still needs some preparation time. This is a known "bug" and Task.Delay(3000); is a nice work-around.
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

                section.Debug("Adding DiscordSocketClient to ServiceCollection");
                serviceCollection.AddSingleton(_discord);

                section.Debug("Adding CommandService to ServiceCollection");
                CommandService commandService = new CommandService(new CommandServiceConfig()
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async
                });
                serviceCollection.AddSingleton(commandService);

                section.Debug("Adding ModuleManager to ServiceCollection");
                serviceCollection.AddSingleton(new ModuleManager(this, commandService, _discord));

                section.Debug("Adding AssetService to ServiceCollection");
                AssetService assetService = new AssetService();
                serviceCollection.AddSingleton(assetService);

                section.Debug("Adding FFmpegService to ServiceCollection");
                FFmpegService FFmpegService = new FFmpegService(assetService);
                serviceCollection.AddSingleton(FFmpegService);

                section.Debug("Adding VoiceService to ServiceCollection");
                serviceCollection.AddSingleton(new VoiceService(FFmpegService));

                section.Debug("Building ServiceProvider");
                ServiceProvider = serviceCollection.BuildServiceProvider();

                //First build ServiceProvider, then create modules (InstallCommandsAsync()) so ServiceProvider is ready to resolve for modules
                ServiceProvider.GetService<ModuleManager>().InstallCommandsAsync().GetAwaiter().GetResult();
            }
        }

        public async Task Shutdown()
        {
            using (LoggingSection section = _localSection.CreateChild("Shutdown"))
            {
                section.Debug("Reached shutdown");
                section.Verbose("Logging out Discord Bot...");
                await _discord.LogoutAsync();
                _discord.Dispose();
                section.Verbose("Saving user-settings...");
                Tokens.Default.Save();
                section.Debug("Marking program as finished...");
                _running = false;
            }

            _localSection.Dispose();
        }

        private Task LogDiscordAsync(Discord.LogMessage logMessage)
        {
            _discordSection.Log(Enum.Parse<LogLevel>(logMessage.Severity.ToString()), logMessage.Message);
            return Task.CompletedTask;
        }
    }
}