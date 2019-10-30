using DiscordYoutubeUpdateListBot.Repository;
using DiscordYoutubeUpdateListBot.Service;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace DiscordYoutubeUpdateListBot
{
    class Program
    {
        private static System.Timers.Timer _timer;

        static async Task Main(string[] args)
        {

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

        private static void SetTimer(IChannelService getChannelInfo)
        {
            _timer = new Timer();
            _timer.Interval = 3600000;
            _timer.Elapsed += (sender, e) => OnTimedEvent(sender, e, getChannelInfo);
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e, IChannelService getChannelInfo)
        {
            new Program().Run(getChannelInfo).Wait();
        }

        private async Task Run(IChannelService getChannelInfo)
        {
            await getChannelInfo.PostVideoUrl();
        }
    }
}
