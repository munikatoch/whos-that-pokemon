using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using WhosThatPokemon.Interfaces.Config;
using WhosThatPokemon.Interfaces.Logger;
using WhosThatPokemon.Interfaces.Repository;
using WhosThatPokemon.Interfaces.Service;
using WhosThatPokemon.Model;
using WhosThatPokemon.Model.DataAccess;
using WhosThatPokemon.Model.Enum;
using WhosThatPokemon.Services.Common;

namespace WhosThatPokemon.Module.Slash
{
    public class GlobalInteractionModule : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly IAppLogger _logger;
        private readonly IUserRepository _userRepository;
        private readonly IDiscordServerRepository _serverRepository;
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IAppConfig _appConfig;
        private readonly IPokemonService _pokemonService;

        public GlobalInteractionModule(IAppLogger logger, IUserRepository userRepository, IDiscordServerRepository serverRepository, IAppConfig appConfig, IPokemonRepository pokemonRepository, IPokemonService pokemonService)
        {
            _logger = logger;
            _userRepository = userRepository;
            _serverRepository = serverRepository;
            _pokemonRepository = pokemonRepository;
            _appConfig = appConfig;
            _pokemonService = pokemonService;
        }

        [SlashCommand("predict", "Predict pokemon")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task PredictPokemon(string url)
        {
            await RespondAsync(Constants.BotPredictPokemonMessage);
            _ = Task.Run(async () =>
            {
                Embed embed = await _pokemonService.PredictPokemon(url);
                await Context.Channel.SendMessageAsync(embed: embed);
            });
            await _logger.CommandUsedLogAsync("GlobalInteractionModule", "predict", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [SlashCommand("collection", "Add or remove pokemons from collection")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Collection(PokemonCollectionOperation operation, string collection = "")
        {
            _ = Task.Run(async () =>
            {
                if (operation == PokemonCollectionOperation.List)
                {
                    await RespondAsync(Constants.BotListCollectionMessage);
                    DiscordUser user = await _userRepository.GetUserByUserId(Context.User.Id);
                    int[]? pokemonIds = user.PokemonCollection;
                    List<Pokemon> pokemons = new List<Pokemon>();
                    if (pokemonIds != null)
                    {
                        pokemons = await _pokemonRepository.GetPokemonById(pokemonIds);

                    }
                    Embed pokemonCollectionListEmbed = DiscordEmbedBuilder.BuildPokemonCollection(pokemons);
                    await Context.Channel.SendMessageAsync(embed: pokemonCollectionListEmbed);
                }
                else if (operation == PokemonCollectionOperation.Add)
                {
                    int nonPremiumUserCollectionLimit = _appConfig.GetValue("UserPokemonCollectionLimit", 0);
                    await RespondAsync(string.Format(Constants.BotUserAddPokemonCollectionnMessage, nonPremiumUserCollectionLimit));
                    List<Pokemon> addedPokemon = await _userRepository.UpsertUserPokemonCollection(Context.User.Id, collection).ConfigureAwait(false);
                    Embed pokemonsAddedEmbed = DiscordEmbedBuilder.BuildAddedPokemonEmbed(addedPokemon, operation);
                    await Context.Channel.SendMessageAsync(embed: pokemonsAddedEmbed);
                }
                else
                {
                    await RespondAsync(Constants.BotUserDeletePokemonCollectionnMessage);
                    List<Pokemon> pokemonsRemoved = await _userRepository.RemoveUserPokemonCollection(Context.User.Id, collection);
                    Embed pokemonsAddedEmbed = DiscordEmbedBuilder.BuildAddedPokemonEmbed(pokemonsRemoved, operation);
                    await Context.Channel.SendMessageAsync(embed: pokemonsAddedEmbed);
                }
            });
            await _logger.CommandUsedLogAsync("GlobalInteractionModule", $"collection {operation}", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [SlashCommand("rareping", "Set pokemon rare ping role")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task RarePing(SocketRole role)
        {
            await RespondAsync(string.Format(Constants.BotSetRarePingMessage, role.Mention));
            _ = Task.Run(async() =>
            {
                await _serverRepository.UpdateRole(Context.Guild.Id, DiscordRoleType.Rare, role.Id).ConfigureAwait(false);
            });
            await _logger.CommandUsedLogAsync("GlobalInteractionModule", "rareping", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [SlashCommand("regionalping", "Set pokemon rare ping role")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task RegionalPing(SocketRole role)
        {
            await RespondAsync(string.Format(Constants.BotSetRegionalPingMessage, role.Mention));

            _ = Task.Run(async () =>
            {
                await _serverRepository.UpdateRole(Context.Guild.Id, DiscordRoleType.Regional, role.Id).ConfigureAwait(false);
            });

            await _logger.CommandUsedLogAsync("GlobalInteractionModule", "regionalping", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [SlashCommand("shadowping", "Set pokemon rare ping role")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task ShadowPing(SocketRole role)
        {
            await RespondAsync(string.Format(Constants.BotSetShadowPingMessage, role.Mention));

            _ = Task.Run(async () =>
            {
                await _serverRepository.UpdateRole(Context.Guild.Id, DiscordRoleType.Shadow, role.Id).ConfigureAwait(false);
            });

            await _logger.CommandUsedLogAsync("GlobalInteractionModule", "shadowping", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [SlashCommand("premium", "Donate to whos that pokemon bot")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Premium()
        {
            Embed premiumEmbed = DiscordEmbedBuilder.BuildPremiumCommandEmbed();
            await RespondAsync(embed: premiumEmbed).ConfigureAwait(false);
            await _logger.CommandUsedLogAsync("GlobalInteractionModule", "premium", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [SlashCommand("help", "Get all available commands")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Help()
        {
            Embed helpEmbed = DiscordEmbedBuilder.BuildHelpCommandEmbed();
            await RespondAsync(embed: helpEmbed).ConfigureAwait(false);
            await _logger.CommandUsedLogAsync("GlobalInteractionModule", "help", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }
    }
}
