using DiscordYoutubeUpdateListBot.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordYoutubeUpdateListBot.Repository
{
    public interface IChannelRepository
    {
        Task<List<string>> GetListChannelUrl();

        Task AddChannelId(string channelId, string channelName);

        Task DeleteChannel(int no);

        Task<string> GetChannelName(string channelId);

        Task<List<ChannelData>> GetAllChannels();
    }
}
