using Discord;
using System.Threading.Tasks;

namespace SpackiBot.Services.VoiceService
{
    public class VoiceRequest
    {
        public IGuildUser Requestor { get; }
        public IVoiceChannel VoiceChannel { get; private set; }
        public string File { get; }

        public VoiceRequest(IGuildUser requestor, IVoiceChannel voiceChannel, string file)
        {
            Requestor = requestor;
            VoiceChannel = voiceChannel;
            File = file;
        }

        public async Task<bool> IsValidAsync()
        {
            if (VoiceChannel == null)
            {
                await Requestor.Guild.DownloadUsersAsync();
                VoiceChannel = Requestor.VoiceChannel;
                return VoiceChannel != null;
            }
            else return true;
        }
    }
}