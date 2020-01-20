using Discord;
using Discord.Audio;
using SpackiBot.Logging;
using SpackiBot.Services.FFmpeg;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SpackiBot.Services.VoiceService
{
    public class VoiceService : IServiceStatus
    {
        public ServiceStatus ServiceStatus => _serviceStatus;

        private LoggingSection _loggingSection;

        private FFmpegService _FFmpegService;

        private ServiceStatus _serviceStatus;
        private ConcurrentDictionary<IGuild, VoiceRequestHandler> _voiceRequestQueues;
        private Thread _queueWorker;

        public VoiceService(FFmpegService FFmpegService)
        {
            _serviceStatus = ServiceStatus.Starting;
            _loggingSection = new LoggingSection("Voice-Service");

            _FFmpegService = FFmpegService;

            _voiceRequestQueues = new ConcurrentDictionary<IGuild, VoiceRequestHandler>();

            _queueWorker = new Thread(HandleQueue) { IsBackground = true }; //IsBackground = true lets the thread automatically shutdown on program exit, so we don't have to handle it anymore

            _queueWorker.Start();
            _serviceStatus = ServiceStatus.Enabled;
        }

        public VoiceRequestHandler GetVoiceRequestQueue(IGuild guild)
        {
            VoiceRequestHandler queue;
            if (_voiceRequestQueues.ContainsKey(guild))
                queue = _voiceRequestQueues[guild];
            else
            {
                queue = new VoiceRequestHandler(this, guild);
                _voiceRequestQueues[guild] = queue;
            }

            return queue;
        }

        public void Request(VoiceRequest voiceRequest) => GetVoiceRequestQueue(voiceRequest.Requestor.Guild).Enqueue(voiceRequest);

        private void HandleQueue(object obj)
        {
            while (true)
            {
                foreach (var pair in _voiceRequestQueues)
                {
                    VoiceRequestHandler queue = pair.Value;

                    if (!queue.IsWorking)
                        _ = queue.HandleNext();
                }
            }
        }

        public async Task PlayInVoiceAsync(IVoiceChannel voiceChannel, string file)
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
    }
}