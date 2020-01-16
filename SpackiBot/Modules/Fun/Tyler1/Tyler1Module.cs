using Discord;
using Discord.Audio;
using Discord.Commands;
using SpackiBot.Logging;
using SpackiBot.Services.AssetService;
using SpackiBot.Services.FFmpeg;
using SpackiBot.Services.VoiceService;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace SpackiBot.Modules.Fun.Tyler1
{
    [Name("Tyler")]
    [Group("tyler")]
    [Alias(new string[] { "t", "deformed" })]
    public class Tyler1Module : ModuleBase<SocketCommandContext>, IModuleStatus
    {
        public ModuleStatus ServiceStatus { get => _moduleStatus; }

        private LoggingSection _loggingSection;

        private AssetService _assetService;
        private VoiceService _voiceService;

        private ModuleStatus _moduleStatus;
        private AssetFolder _assetFolder;

        public Tyler1Module(AssetService assetService, VoiceService voiceService)
        {
            _assetService = assetService;
            _voiceService = voiceService;

            _moduleStatus = ModuleStatus.Starting;
            _loggingSection = new LoggingSection("Module Tyler1");

            try
            {
                _assetFolder = assetService.FindSub(new string[] { "Sounds", "Tyler1" }, StringComparison.Ordinal);
            }
            catch (DirectoryNotFoundException)
            {
                _moduleStatus = ModuleStatus.Failed;
                _loggingSection.Error($"Could not find folder for Tyler1 sounds.");
            }

            _moduleStatus = ModuleStatus.Enabled;
        }

        [Name("Tyler")]
        [Command()]
        [Summary("Tyler motherfucking one")]
        public async Task Tyler()
        {
            Task deleteAsync = Context.Message.DeleteAsync();
            Task replyAsync = ReplyAsync("Tyler motherfucking one");

            await deleteAsync;
            await replyAsync;
        }

        [Command("Play")]
        [Summary("Spielt einen Tyler1-Sound in deinem Voice-Channel")]
        public async Task PlayAsync([Summary("(Optional) Filter für bestimmten Sound")] [Remainder] string filter = null)
        {
            if (_moduleStatus == ModuleStatus.Failed)
            {
                await ReplyAsync("Sorry, da es einen Fehler beim Initialisieren des Tyler1-Moduls gab, ist dieser Befehl gerade deaktiviert");
                return;
            }

            string file = (filter == null ? _assetFolder.RandomFile() : _assetFolder.FilterFiles(filter, StringComparison.OrdinalIgnoreCase).FirstOrDefault());
            if (file == null)
                await ReplyAsync("Die gewünschte Tyler1-Quote wurde leider nicht gefunden oder ist noch nicht in meiner Datenbank registriert");
            else
            {
                if (_voiceService.IsWorking)
                    _ = ReplyAsync("Tyler ist gerade beschäftigt, aber er wird schon bald auch bei dir vorbeischauen!");
                _voiceService.Request(new VoiceRequest(Context.User, (Context.User as IGuildUser).VoiceChannel, file));
            }
        }

        [Command("Quote")]
        [Summary("Gibt ein Tyler1-Zitat aus")]
        public async Task QuoteAsync([Summary("(Optional) Filter für bestimmtes Zitat")] [Remainder] string filter = null)
        {
            await ReplyAsync("Dieses Feature muss noch programmiert werden!");
        }
    }
}