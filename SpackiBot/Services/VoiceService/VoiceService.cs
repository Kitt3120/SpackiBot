using Discord;
using Discord.Audio;
using Discord.Commands;
using SpackiBot.Logging;
using SpackiBot.Services.FFmpeg;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpackiBot.Services.VoiceService
{
    public class VoiceService : IServiceStatus
    {
        public ServiceStatus ServiceStatus => _serviceStatus;
        public bool IsWorking { get; private set; }

        private LoggingSection _loggingSection;

        private FFmpegService _FFmpegService;

        private ServiceStatus _serviceStatus;
        private ConcurrentQueue<VoiceRequest> _requestQueue;
        private Thread _requestQueueWorker;

        public VoiceService(FFmpegService FFmpegService)
        {
            _serviceStatus = ServiceStatus.Starting;
            _loggingSection = new LoggingSection("Voice-Service");

            _FFmpegService = FFmpegService;

            _requestQueue = new ConcurrentQueue<VoiceRequest>();
            //IsBackground = true lets the thread automatically shutdown on program exit, so we don't have to handle it anymore
            _requestQueueWorker = new Thread(HandleQueue) { IsBackground = true };
            IsWorking = false;

            _requestQueueWorker.Start();
            _serviceStatus = ServiceStatus.Enabled;
        }

        public void Request(VoiceRequest voiceRequest) => _requestQueue.Enqueue(voiceRequest);

        private async void HandleQueue(object obj)
        {
            while (true)
            {
                if (!IsWorking)
                {
                    if (_requestQueue.TryDequeue(out VoiceRequest voiceRequest))
                    {
                        IsWorking = true;
                        _loggingSection.Debug("Begin");

                        try
                        {
                            _loggingSection.Debug("Got voice");
                            _loggingSection.Debug("Playing...");

                            if (!(await voiceRequest.IsValidAsync()))
                                await (await voiceRequest.Requestor.GetOrCreateDMChannelAsync()).SendMessageAsync("Deine Anfrage wurde übersprungen, da du in keinem VoiceChannel mehr warst!");
                            else
                                await PlayInVoiceAsync(voiceRequest.VoiceChannel, voiceRequest.File);
                        }
                        catch (Exception)
                        { }
                        finally
                        {
                            _loggingSection.Debug("FINALLY");
                            IsWorking = false;
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
    }
}