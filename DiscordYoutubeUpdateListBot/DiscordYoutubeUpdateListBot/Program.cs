using DiscordYoutubeUpdateListBot.Repository;
using DiscordYoutubeUpdateListBot.Service;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DiscordYoutubeUpdateListBot
{
    class Program
    {
        private static System.Timers.Timer _timer4WakeUp;
        private static System.Timers.Timer _timer4Bot;

        static async Task Main(string[] args)
        {
            SetWakeUpTimer();

            var services = new ServiceCollection();

            services
                .AddTransient<IChannelService, ChannelService>()
                .AddTransient<IChannelRepository, ChannelRepository>();

            var servicesProvider = services.BuildServiceProvider();
            var getChannelInfo = (ChannelService)servicesProvider.GetService<IChannelService>();
            var getChannelData = (ChannelRepository)servicesProvider.GetService<IChannelRepository>();

            SetTimer(getChannelInfo);

            var botToken = Common.Common.GetConfigurationRoot()
                .GetSection("DiscordBotToken").Value;

            var discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = botToken,
                TokenType = TokenType.Bot
            });

            var deps = new DependencyCollectionBuilder();

            deps.AddInstance(getChannelInfo)
                .AddInstance(discordClient)
                .AddInstance(getChannelData);

            var commands = discordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "!yt",
                Dependencies = deps.Build()
            });

            commands.RegisterCommands<ChannelService>();

            await discordClient.ConnectAsync();
            await Task.Delay(-1);
            Console.ReadKey();
        }

        private static void SetWakeUpTimer()
        {
            _timer4WakeUp = new Timer();
            _timer4WakeUp.Interval = 600000;
            _timer4WakeUp.Elapsed += (sender, e) => OnWakeUpTimedEvent(sender, e);
            _timer4WakeUp.AutoReset = true;
            _timer4WakeUp.Enabled = true;
        }

        private static void OnWakeUpTimedEvent(Object source, ElapsedEventArgs e)
        {
            WakeUpWebJob();
            Console.WriteLine("WakeUP!!!");
        }

        private static void SetTimer(IChannelService getChannelInfo)
        {
            _timer4Bot = new Timer();
            _timer4Bot.Interval = 3600000;
            _timer4Bot.Elapsed += (sender, e) => OnTimedEvent(sender, e, getChannelInfo);
            _timer4Bot.AutoReset = true;
            _timer4Bot.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e, IChannelService getChannelInfo)
        {
            new Program().Run(getChannelInfo).Wait();
        }

        private async Task Run(IChannelService getChannelInfo)
        {
            await getChannelInfo.PostVideoUrl();
            Console.WriteLine("CheckVideo");
        }

        private static void WakeUpWebJob()
        {
            string websiteName = "DiscordYoutubePlaylistBot";
            string webjobName = "DiscordYoutubeUpdateListBot";
            string userName = "$DiscordYoutubePlaylistBot";
            string userPWD = "Zd8gkftoGSouAP6cggxTGMErTtYmx16BkvbnaSJGWb8tqFzgjqH0zns44wAp";
            string webjobUrl = string.Format("https://{0}.scm.azurewebsites.net/api/continuouswebjobs/{1}", websiteName, webjobName);
            var result = GetWebjobState(webjobUrl, userName, userPWD);
        }

        private static JObject GetWebjobState(string webjobUrl, string userName, string userPWD)
        {
            HttpClient client = new HttpClient();
            string auth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ':' + userPWD));
            client.DefaultRequestHeaders.Add("authorization", auth);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var data = client.GetStringAsync(webjobUrl).Result;
            var result = JsonConvert.DeserializeObject(data) as JObject;
            return result;
        }
    }
}
