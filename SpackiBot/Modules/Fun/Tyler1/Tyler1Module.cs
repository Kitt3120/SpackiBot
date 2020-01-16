using Discord;
using Discord.Audio;
using Discord.Commands;
using SpackiBot.Logging;
using SpackiBot.Services.AssetService;
using SpackiBot.Services.FFmpeg;
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
        private AssetFolder _assetFolder;
        private FFmpegService _FFmpegService;

        private ModuleStatus _moduleStatus;
        private ConcurrentQueue<(SocketCommandContext, string)> _requestQueue;
        private Thread _requestQueueWorker;
        private bool _isWorking;

        public Tyler1Module(AssetService assetService, FFmpegService FFmpegService)
        {
            _assetService = assetService;
            _FFmpegService = FFmpegService;

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

            _requestQueue = new ConcurrentQueue<(SocketCommandContext, string)>();
            //IsBackground = true lets the thread automatically shutdown on program exit, so we don't have to handle it anymore
            _requestQueueWorker = new Thread(HandleQueue) { IsBackground = true };
            _isWorking = false;

            _requestQueueWorker.Start();
            _moduleStatus = ModuleStatus.Enabled;
        }

        private async void HandleQueue(object obj)
        {
            while (true)
            {
                if (!_isWorking)
                {
                    if (_requestQueue.TryDequeue(out (SocketCommandContext, string) result))
                    {
                        _isWorking = true;
                        _loggingSection.Debug("Begin");
                        SocketCommandContext context = result.Item1;
                        string file = result.Item2;

                        try
                        {
                            IVoiceChannel voiceChannel = (context.User as IGuildUser).VoiceChannel;
                            _loggingSection.Debug("Got voice");
                            _loggingSection.Debug("Playing...");
                            if (voiceChannel == null)
                                await (await context.User.GetOrCreateDMChannelAsync()).SendMessageAsync("Deine Anfrage wurde übersprungen, da du in keinem VoiceChannel mehr warst!");
                            else
                                await PlayInVoiceAsync(voiceChannel, file);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            _loggingSection.Debug("FINALLY");
                            _isWorking = false;
                        }
                    }
                }
            }
        }

        private async Task PlayInVoiceAsync(IVoiceChannel voiceChannel, string file)
        {
            using (var audioClient = await voiceChannel.ConnectAsync())
            using (var ffmpeg = _FFmpegService.ReadAudio(file))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var audioStream = audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(audioStream); }
                finally { await audioStream.FlushAsync(); }
            }
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
                if (_isWorking)
                    _ = ReplyAsync("Tyler ist gerade beschäftigt, aber er wird schon bald auch bei dir vorbeischauen!");
                _requestQueue.Enqueue((Context, file));
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