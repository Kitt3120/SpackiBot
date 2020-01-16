using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpackiBot.Services.VoiceService
{
    public class VoiceRequest
    {
        public IUser Requestor { get; }
        public IVoiceChannel VoiceChannel { get; }
        public string File { get; }

        public VoiceRequest(IUser requestor, IVoiceChannel voiceChannel, string file)
        {
            Requestor = requestor;
            VoiceChannel = voiceChannel;
            File = file;
        }

        public async Task<bool> IsValidAsync()
        {
            bool contains = false;
            List<IReadOnlyCollection<IGuildUser>> userCollections = await VoiceChannel.GetUsersAsync().ToList();
            foreach (IReadOnlyCollection<IUser> userCollection in userCollections)
            {
                foreach (IUser user in userCollection)
                {
                    if (user.Id == Requestor.Id)
                        contains = true;
                }
            }

            return contains;
        }
    }
}