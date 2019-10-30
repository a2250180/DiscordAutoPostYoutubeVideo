using DSharpPlus;
using Firebase.Auth;
using Firebase.Database;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace DiscordYoutubeUpdateListBot.Common
{
    public static class Common
    {
        public static IConfigurationRoot GetConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();
        }

        public static DiscordClient GetDiscordClient()
        {
            var botToken = GetConfigurationRoot()
                .GetSection("DiscordBotToken").Value;

            var discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = botToken,
                TokenType = TokenType.Bot
            });

            return discordClient;
        }

        public static async Task<FirebaseClient> GetClient()
        {
            var configuration = GetConfigurationRoot();
            var authProvider = new FirebaseAuthProvider(
                new FirebaseConfig(configuration.GetSection("FirebaseApiKey").Value));

            var mail = configuration.GetSection("FirebaseAuthMail").Value;
            var password = configuration.GetSection("FirebaseAuthPassword").Value;

            var auth = await authProvider.SignInWithEmailAndPasswordAsync(mail, password);

            var client =
                new FirebaseClient(
                    "https://discordbot-f1221.firebaseio.com/",
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(auth.FirebaseToken)
                    });

            return client;
        }
    }
}
