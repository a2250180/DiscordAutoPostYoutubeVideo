using DiscordYoutubeUpdateListBot.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordYoutubeUpdateListBot.Repository
{
    public class ChannelRepository : IChannelRepository
    {
        public async Task<Dictionary<ulong, List<string>>> GetListChannelUrl(ulong discordChannelId)
        {
            var googleApiKey = Common.Common
                .GetConfigurationRoot()
                .GetSection("GoogleApiKey").Value;
            var listVideoUrl = new List<string>();
            var dicChannelVideoUrl = new Dictionary<ulong, List<string>>();
            var channelIds = (await GetAllChannels(discordChannelId)).Select(x => x.ChannelId);

            foreach (var channelId in channelIds)
            {
                var requestUrl =
                    $"https://www.googleapis.com/youtube/v3/search?key={googleApiKey}&channelId={channelId}&part=snippet,id&order=date&maxResults=5&type=video";

                var httpClient = new HttpClient();

                var response = await httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var objContent = JsonConvert.DeserializeObject<JObject>(content);
                    var objContentItems = objContent["items"].Value<JArray>();

                    if(objContentItems != null)
                    {
                        foreach (var item in objContentItems)
                        {
                            var id = item["id"].Value<JObject>();
                            var videoId = id["videoId"].ToString();

                            var snippet = item["snippet"].Value<JObject>();
                            var publishedAt = Convert.ToDateTime(snippet["publishedAt"].ToString());

                            if (publishedAt > DateTime.UtcNow.AddHours(-1) && publishedAt < DateTime.UtcNow)
                            {
                                listVideoUrl.Add(videoId);
                            }
                        }
                    }

                }
            }

            if (listVideoUrl.Count > 0)
            {
                dicChannelVideoUrl.Add(discordChannelId, listVideoUrl);
            }

            return dicChannelVideoUrl;
        }

        public async Task AddChannelId(string channelId, string channelName, ulong discordChannelId)
        {
            var child = (await Common.Common.GetClient()).Child($"ChannelData/{discordChannelId.ToString()}");

            var data = new ChannelData()
            {
                Id = Guid.NewGuid(),
                ChannelId = channelId,
                ChannelName = channelName
            };

            await child.PostAsync(JsonConvert.SerializeObject(data));
        }

        public async Task DeleteChannel(int no, ulong discordChannelId)
        {
            var parent = (await Common.Common.GetClient()).Child($"ChannelData/{discordChannelId}");

            var allChannels = await parent.OnceAsync<ChannelData>();

            var currentChannel = allChannels.ToList()[no - 1];

            var currentChild = (await Common.Common.GetClient()).Child($"ChannelData/{currentChannel?.Key}");

            await currentChild.DeleteAsync();
        }

        public async Task<string> GetChannelName(string channelId)
        {
            var googleApiKey = Common.Common
                .GetConfigurationRoot()
                .GetSection("GoogleApiKey").Value;
            var listVideoUrl = new List<string>();
            var requestUrl =
                    $"https://www.googleapis.com/youtube/v3/search?key={googleApiKey}&channelId={channelId}&part=snippet,id&order=date&maxResults=1";

            var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var objContent = JsonConvert.DeserializeObject<JObject>(content);
                var objContentItems = objContent["items"].Value<JArray>();

                var snippet = objContentItems[0]["snippet"].Value<JObject>();
                var channelTitle = snippet["channelTitle"].Value<string>();

                return channelTitle;
            }

            return string.Empty;
        }

        public async Task<List<ChannelData>> GetAllChannels(ulong discordChannelId)
        {
            var child = (await Common.Common.GetClient()).Child($"ChannelData/{discordChannelId.ToString()}");

            var allChannels = await child.OnceAsync<ChannelData>();

            return allChannels.Select(x => x.Object).ToList();
        }

        public async Task<List<ulong>> GetAllDiscordChannelId()
        {
            var child = (await Common.Common.GetClient()).Child("ChannelData");

            var allDiscordChannels = await child.OnceAsync<object>();

            return allDiscordChannels.Select(x => Convert.ToUInt64(x.Key)).ToList();
        }
    }
}
