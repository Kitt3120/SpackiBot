using Discord;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SpackiBot.Services.VoiceService
{
    public class VoiceRequestHandler
    {
        public IGuild Guild { get; }
        public bool IsWorking { get; private set; }

        private VoiceService _voiceService;
        private ConcurrentQueue<VoiceRequest> _requestQueue;

        public VoiceRequestHandler(VoiceService voiceService, IGuild guild)
        {
            _voiceService = voiceService;
            _requestQueue = new ConcurrentQueue<VoiceRequest>();
            Guild = guild;
            IsWorking = false;
        }

        public void Enqueue(VoiceRequest voiceRequest)
        {
            if (voiceRequest.Requestor.Guild != Guild)
                throw new InvalidOperationException("VoiceRequest guild is wrong");

            _requestQueue.Enqueue(voiceRequest);
        }

        public async Task HandleNext()
        {
            if (_requestQueue.TryDequeue(out VoiceRequest voiceRequest))
            {
                IsWorking = true;

                try
                {
                    if (!(await voiceRequest.IsValidAsync()))
                        await (await voiceRequest.Requestor.GetOrCreateDMChannelAsync()).SendMessageAsync("Deine Anfrage wurde übersprungen, da du in keinem VoiceChannel mehr warst!");
                    else
                        await _voiceService.PlayInVoiceAsync(voiceRequest.VoiceChannel, voiceRequest.File);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    IsWorking = false;
                }
            }
        }
    }
}