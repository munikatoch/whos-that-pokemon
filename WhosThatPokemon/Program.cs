using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using MongoDB.Driver;
using Serilog;
using Serilog.Exceptions;
using WhosThatPokemon.Config;
using WhosThatPokemon.Handler;
using WhosThatPokemon.Interfaces.Config;
using WhosThatPokemon.Interfaces.Discord;
using WhosThatPokemon.Interfaces.Log;
using WhosThatPokemon.Interfaces.Repository;
using WhosThatPokemon.Interfaces.Service;
using WhosThatPokemon.Logging;
using WhosThatPokemon.Model;
using WhosThatPokemon.Model.Enum;
using WhosThatPokemon.Model.MachineLearning;
using WhosThatPokemon.Repository.MongoDB;
using WhosThatPokemon.Services.Common;
using WhosThatPokemon.Services.Discord;

namespace WhosThatPokemon
{
    public class Program
    {
        static void Main(string[] args)
        {
            IServiceProvider provider = RegisterContainer();
            IDiscordBot? bot = provider.GetService<IDiscordBot>();
            if (bot != null)
            {
                bot.ConnectAndStartBot().GetAwaiter().GetResult();
            }
        }

        private static IServiceProvider RegisterContainer()
        {
            IServiceCollection collection = new ServiceCollection();
            ILogger logger = CreateLoggerConfiguation();

            #region Config

            collection.AddSingleton(x => new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.MessageContent |
                                GatewayIntents.AllUnprivileged,
                LogLevel = LogSeverity.Warning,
            });

            collection.AddSingleton(logger);
            collection.AddSingleton<IAppConfig, AppConfig>();

            #endregion

            #region Discord

            collection.AddSingleton<IDiscordBot, DiscordBot>();
            collection.AddSingleton(x => new DiscordShardedClient(x.GetService<DiscordSocketConfig>()));
            collection.AddSingleton<CommandService>();
            collection.AddSingleton<InteractionService>();
            collection.AddScoped<IPrefixHandler, PrefixCommandHandler>();
            collection.AddScoped<IInteractionHandler, InteractionCommandHandler>();

            #endregion

            #region Database

            collection.AddSingleton(x => new MongoClient(x.GetRequiredService<IAppConfig>().GetValue("MongoDBConnectionString", string.Empty)));
            collection.AddScoped<IUserRepository, UserRepository>();
            collection.AddScoped<IPokemonRepository, PokemonRepository>();
            collection.AddScoped<IDiscordServerRepository, DiscordServerRepository>();

            #endregion

            collection.AddPredictionEnginePool<ModelInput, ModelOutput>().FromFile(Constants.MlModelFileOutputPath);
            collection.AddScoped<IPokemonService, PokemonService>();
            collection.AddScoped<IHttpHelper, HttpHelper>();
            collection.AddSingleton<IAppLogger, FileLogger>();

            collection.AddHttpClient(HttpClientType.Pokemon.ToString(), x =>
            {
                x.Timeout = TimeSpan.FromSeconds(3);
            });

            return collection.BuildServiceProvider();
        }

        private static ILogger CreateLoggerConfiguation()
        {
            return new LoggerConfiguration().WriteTo.File(Constants.Logfile, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
                .Enrich.WithExceptionDetails()
                .CreateLogger();
        }
    }
}