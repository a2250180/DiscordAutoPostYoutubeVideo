using DiscordYoutubeUpdateListBot.Model;
using DiscordYoutubeUpdateListBot.Repository;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordYoutubeUpdateListBot.Service
{
    public class ChannelService : IChannelService
    {
        DiscordClient discordClient = Common.Common.GetDiscordClient();
        IChannelRepository _getChannelData;

        public ChannelService(IChannelRepository getChannelData)
        {
            _getChannelData = getChannelData;
        }

        public async Task PostVideoUrl()
        {
            var youtubeUrlFormat = "https://www.youtube.com/watch?v={0}";
            var allChannelVideoUrl = await GetChannelVideoId();
            var sb = new StringBuilder();

            var discordChannelId = Common.Common
                .GetConfigurationRoot()
                .GetSection("DiscordChannelId").Value;

            var discordChannel = await discordClient.GetChannelAsync(Convert.ToUInt64(discordChannelId));

            foreach (var url in allChannelVideoUrl)
            {
                sb.AppendLine(string.Format(youtubeUrlFormat, url));
            }

            await discordClient.SendMessageAsync(discordChannel, sb.ToString());
        }

        [Command("add")]
        public async Task StoreChannelId(CommandContext ctx, string msg)
        {
            var msgSplit = msg.Split('/');
            var channelName = await GetChannelName(msgSplit[msgSplit.Length - 1]);

            if (string.IsNullOrEmpty(channelName).Equals(false))
            {
                var allChannelNames = (await _getChannelData.GetAllChannels()).Select(x => x.ChannelName);
                if (allChannelNames.Contains(channelName).Equals(false))
                {
                    await _getChannelData.AddChannelId(msgSplit[msgSplit.Length - 1], channelName);
                    await ctx.Message.RespondAsync($"{channelName}已新增!");
                }
                else
                {
                    await ctx.Message.RespondAsync($"{channelName}已經在列表中了!");
                }
            }
        }

        [Command("list")]
        public async Task GetAll(CommandContext ctx)
        {
            var allChannel = await _getChannelData.GetAllChannels();
            var counter = 1;
            StringBuilder sb = new StringBuilder();
            
            foreach(var c in allChannel)
            {
                sb.AppendLine($"{counter++}. {c.ChannelName}");
            }

            await ctx.Message.RespondAsync(sb.ToString());
        }

        [Command("del")]
        public async Task Delete(CommandContext ctx, int no)
        {
            await _getChannelData.DeleteChannel(no);
            await GetAll(ctx);
        }


        [Command("Help")]
        public async Task Help(CommandContext ctx)
        {
            string helpMsg =
                "全部頻道：!ytlist\r\n" +
                "新增頻道：!ytadd [頻道連結]\r\n" +
                "刪除頻道：!ytdel [頻道號碼]";

            await ctx.Message.RespondAsync(helpMsg);
        }

        private async Task<List<string>> GetChannelVideoId()
        {
            return await _getChannelData.GetListChannelUrl();
        }

        private async Task<string> GetChannelName(string channelId)
        {
            return await _getChannelData.GetChannelName(channelId);
        }
    }
}
