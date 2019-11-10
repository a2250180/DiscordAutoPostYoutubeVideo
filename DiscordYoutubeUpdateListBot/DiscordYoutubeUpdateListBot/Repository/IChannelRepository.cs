using DiscordYoutubeUpdateListBot.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordYoutubeUpdateListBot.Repository
{
    public interface IChannelRepository
    {
        Task<Dictionary<ulong, List<string>>> GetListChannelUrl(ulong discordChannelId);

        Task AddChannelId(string channelId, string channelName, ulong discordChannelId);

        Task DeleteChannel(int no, ulong discordChannelId);

        Task<string> GetChannelName(string channelId);

        Task<List<ChannelData>> GetAllChannels(ulong discordChannelId);

        Task<List<ulong>> GetAllDiscordChannelId();
    }
}
