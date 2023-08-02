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
using WhosThatPokemon.Services.Common;

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
                else if (_pokemonService.ValidateIsShinyPokemonMessage(message))
                {
                    await SendShinyMessageToStarBoard(message).ConfigureAwait(false);
                }
            }
            else
            {
                await ExecutePrefixCommand(message).ConfigureAwait(false);
            }
        }

        private async Task SendShinyMessageToStarBoard(SocketUserMessage message)
        {
            if (message.Channel is SocketGuildChannel)
            {
                SocketGuildChannel? guildChannel = message.Channel as SocketGuildChannel;
                if (guildChannel != null)
                {
                    DiscordServer server = await _serverRepository.GetServerDataAsync(guildChannel.Guild.Id);
                    await SendMessageToStarboard(message, guildChannel.Guild.Id, server.StarboardChannelId);
                }
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
                if (predictedPokemonResult?.Pokemon != null)
                {
                    Pokemon predictedPokemon = predictedPokemonResult.Pokemon;
                    string roleMention = string.Empty;

                    if (message.Channel is SocketGuild)
                    {
                        SocketGuild? guild = message.Channel as SocketGuild;
                        if (guild != null)
                        {
                            if ((predictedPokemon.IsRare || predictedPokemon.IsShadow || predictedPokemon.IsRegional))
                            {
                                DiscordServer server = await _serverRepository.GetServerDataAsync(guild.Id);
                                if (predictedPokemon.IsRare)
                                {
                                    if (server.RarePingId != 0)
                                    {
                                        roleMention = MentionUtils.MentionRole(server.RarePingId);
                                    }
                                    await SendMessageToStarboard(message, guild.Id, server.StarboardChannelId);
                                }
                                else if (predictedPokemon.IsShadow && server.ShadowPingId != 0)
                                {
                                    roleMention = MentionUtils.MentionRole(server.ShadowPingId);
                                }
                                else if (server.RegionalPingId != 0)
                                {
                                    roleMention = MentionUtils.MentionRole(server.RegionalPingId);
                                }
                            }
                            List<DiscordUser> users = await _userRepository.GetPokemonCollectingUser(predictedPokemon.PokemonId);
                            if (users != null && users.Count > 0)
                            {
                                StringBuilder sb = new StringBuilder("**Collections: **");

                                foreach (DiscordUser user in users)
                                {
                                    var guildUser = guild.GetUser(user.DiscordUserId);
                                    if (guildUser != null)
                                    {
                                        sb.Append(MentionUtils.MentionUser(user.DiscordUserId) + " ");
                                    }
                                }
                                roleMention = sb.ToString();
                            }
                        }
                        await message.ReplyAsync(roleMention, embed: predictedPokemonResult.PokemonEmbed);
                    }
                }
            }
        }

        private async Task SendMessageToStarboard(SocketUserMessage message, ulong guildId, ulong channelId)
        {
            if (channelId != 0)
            {
                SocketGuild guild = _client.GetGuild(guildId);
                if (guild != null)
                {
                    SocketGuildChannel channel = guild.GetChannel(channelId);
                    if (channel is SocketTextChannel)
                    {
                        SocketTextChannel textChannel = (SocketTextChannel)channel;
                        Embed embed = DiscordEmbedBuilder.BuildStartboardEmbed(message, message.GetJumpUrl());
                        await textChannel.SendMessageAsync(embed: embed);
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
