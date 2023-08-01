using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Serilog.Events;
using System.Text;
using WhosThatPokemon.Interfaces.Discord;
using WhosThatPokemon.Interfaces.Logger;
using WhosThatPokemon.Interfaces.Repository;
using WhosThatPokemon.Interfaces.Service;
using WhosThatPokemon.Model;
using WhosThatPokemon.Model.DataAccess;
using WhosThatPokemon.Module.Prefix;

namespace WhosThatPokemon.Handler
{
    public class PrefixCommandHandler : IPrefixHandler
    {
        private readonly DiscordShardedClient _client;
        private readonly CommandService _commandService;
        private readonly IAppLogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPokemonService _pokemonService;
        private readonly IDiscordServerRepository _serverRepository;
        private readonly IUserRepository _userRepository;

        public PrefixCommandHandler(
            DiscordShardedClient client, 
            CommandService commandService, 
            IAppLogger logger, 
            IServiceProvider serviceProvider, 
            IPokemonService pokemonService, 
            IDiscordServerRepository serverRepository,
            IUserRepository userRepository
            )
        {
            _client = client;
            _commandService = commandService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _pokemonService = pokemonService;
            _serverRepository = serverRepository;
            _userRepository = userRepository;
        }
        public async Task InitializeAsync()
        {
            ModuleInfo globalModule = await _commandService.AddModuleAsync<GlobalPrefixModule>(_serviceProvider);

            _commandService.Log += LogCommandServiceEvent;
            _client.MessageReceived += MessageReceivedEvent;
        }

        private Task MessageReceivedEvent(SocketMessage socketMessage)
        {
            if (socketMessage != null && socketMessage is SocketUserMessage message)
            {
                _ = Task.Run(async () =>
                {
                    await HandleCommandAsync(message);
                });
            }
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketUserMessage message)
        {
            if (message.Author.IsBot)
            {
                if (_pokemonService.ValidatePokemonSpanMessage(message))
                {
                    await TryGetPokemonPrediction(message).ConfigureAwait(false);
                }
            }
            else
            {
                await ExecutePrefixCommand(message).ConfigureAwait(false);
            }
        }

        private async Task ExecutePrefixCommand(SocketUserMessage message)
        {
            int argPos = 0;
            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                ShardedCommandContext context = new ShardedCommandContext(_client, message);
                IResult result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
                if (!result.IsSuccess)
                {
                    await _logger.FileLogAsync(result, LogEventLevel.Error).ConfigureAwait(false);
                    await message.ReplyAsync(result.ErrorReason);
                }
            }
        }

        private async Task TryGetPokemonPrediction(SocketUserMessage message)
        {
            Embed? embed = message.Embeds.FirstOrDefault();
            if (embed?.Image != null && embed.Image.HasValue)
            {
                DiscordPokemonPrediction predictedPokemonResult = await _pokemonService.PredictSpawnedPokemon(embed.Image.Value.Url, embed.Color);
                if(predictedPokemonResult?.Pokemon != null)
                {
                    Pokemon predictedPokemon = predictedPokemonResult.Pokemon;
                    string roleMention = string.Empty;

                    if(message.Channel is SocketGuildChannel)
                    {
                        SocketGuildChannel? channel = message.Channel as SocketGuildChannel;
                        if ((predictedPokemon.IsRare || predictedPokemon.IsShadow || predictedPokemon.IsRegional) && channel?.Guild != null)
                        {
                            DiscordServer server = await _serverRepository.GetMentionRoles(channel.Guild.Id);
                            if (predictedPokemon.IsRare)
                            {
                                roleMention = MentionUtils.MentionRole(server.RarePingId);
                            }
                            else if (predictedPokemon.IsShadow)
                            {
                                roleMention = MentionUtils.MentionRole(server.ShadowPingId);
                            }
                            else
                            {
                                roleMention = MentionUtils.MentionRole(server.RegionalPingId);
                            }
                        }
                        else
                        {
                            List<DiscordUser> users = await _userRepository.GetPokemonCollectingUser(predictedPokemon.PokemonId);
                            if (users != null && users.Count > 0)
                            {
                                StringBuilder sb = new StringBuilder("**Collections: **");
                                foreach (DiscordUser user in users)
                                {
                                    sb.Append(MentionUtils.MentionUser(user.DiscordUserId));
                                }
                                roleMention = sb.ToString();
                            }
                        }
                        await message.ReplyAsync(roleMention, embed: predictedPokemonResult.PokemonEmbed);
                    }
                }
            }
        }

        private async Task LogCommandServiceEvent(LogMessage log)
        {
            Enum.TryParse(log.Severity.ToString(), out LogEventLevel logLevel);
            await _logger.FileLogAsync(log, logLevel).ConfigureAwait(false);
        }
    }
}
