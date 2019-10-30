using DSharpPlus.CommandsNext;
using System.Threading.Tasks;

namespace DiscordYoutubeUpdateListBot.Service
{
    public interface IChannelService
    {
        Task PostVideoUrl();

        Task StoreChannelId(CommandContext ctx, string msg);

        Task Delete(CommandContext ctx, int no);
    }
}
