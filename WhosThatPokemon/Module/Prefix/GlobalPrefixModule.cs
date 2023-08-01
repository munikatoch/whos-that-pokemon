using Discord;
using Discord.Commands;
using Discord.WebSocket;
using WhosThatPokemon.Interfaces.Config;
using WhosThatPokemon.Interfaces.Logger;
using WhosThatPokemon.Interfaces.Repository;
using WhosThatPokemon.Interfaces.Service;
using WhosThatPokemon.Model;
using WhosThatPokemon.Model.DataAccess;
using WhosThatPokemon.Model.Enum;
using WhosThatPokemon.Services.Common;

namespace WhosThatPokemon.Module.Prefix
{
    public class GlobalPrefixModule : ModuleBase<ShardedCommandContext>
    {
        private readonly IAppLogger _logger;
        private readonly IUserRepository _userRepository;
        private readonly IDiscordServerRepository _serverRepository;
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IAppConfig _appConfig;
        private readonly IPokemonService _pokemonService;

        public GlobalPrefixModule(IAppLogger logger, IUserRepository userRepository, IDiscordServerRepository serverRepository, IPokemonRepository pokemonRepository, IAppConfig appConfig, IPokemonService pokemonService)
        {
            _logger = logger;
            _userRepository = userRepository;
            _serverRepository = serverRepository;
            _pokemonRepository = pokemonRepository;
            _appConfig = appConfig;
            _pokemonService = pokemonService;
        }

        [Command("predict")]
        [Alias("p")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task PredictPokemon(string url)
        {
            await Context.Message.ReplyAsync(Constants.BotPredictPokemonMessage);
            _ = Task.Run(async () =>
            {
                Embed embed = await _pokemonService.PredictPokemon(url);
                await Context.Channel.SendMessageAsync(embed: embed);
            });
            await _logger.CommandUsedLogAsync("GlobalPrefixModule", "predict", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [Command("collection", true)]
        [Alias("collect", "cl", "c")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Collection(PokemonCollectionOperation operation, params string[] collection)
        {
            _ = Task.Run(async () =>
            {
                if (operation == PokemonCollectionOperation.List)
                {
                    await Context.Message.ReplyAsync(Constants.BotListCollectionMessage);
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
                    await Context.Message.ReplyAsync(string.Format(Constants.BotUserAddPokemonCollectionnMessage, nonPremiumUserCollectionLimit));
                    List<Pokemon> addedPokemon = await _userRepository.UpsertUserPokemonCollection(Context.User.Id, string.Join(" ", collection));
                    Embed pokemonsAddedEmbed = DiscordEmbedBuilder.BuildAddedPokemonEmbed(addedPokemon);
                    await Context.Channel.SendMessageAsync(embed: pokemonsAddedEmbed);
                }
                else
                {
                    await Context.Message.ReplyAsync(Constants.BotUserDeletePokemonCollectionnMessage);
                    await _userRepository.RemoveUserPokemonCollection(Context.User.Id, string.Join(" ", collection)).ConfigureAwait(false);

                }
            });
            await _logger.CommandUsedLogAsync("GlobalPrefixModule", "collection", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [Command("rareping")]
        [Alias("rp")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task RarePing(SocketRole role)
        {
            await Context.Message.ReplyAsync(string.Format(Constants.BotSetRarePingMessage, role.Mention));
            _ = Task.Run(async () =>
            {
                await _serverRepository.UpdateRole(Context.Guild.Id, DiscordRoleType.Rare, role.Id).ConfigureAwait(false);
            });
            await _logger.CommandUsedLogAsync("GlobalPrefixModule", "rareping", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [Command("regionalping")]
        [Alias("rgp")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task RegionalPing(SocketRole role)
        {
            await Context.Message.ReplyAsync(string.Format(Constants.BotSetRegionalPingMessage, role.Mention));

            _ = Task.Run(async () =>
            {
                await _serverRepository.UpdateRole(Context.Guild.Id, DiscordRoleType.Regional, role.Id).ConfigureAwait(false);
            });

            await _logger.CommandUsedLogAsync("GlobalPrefixModule", "regionalping", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [Command("shadowping")]
        [Alias("sp")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task ShadowPing(SocketRole role)
        {
            await Context.Message.ReplyAsync(string.Format(Constants.BotSetShadowPingMessage, role.Mention));

            _ = Task.Run(async () =>
            {
                await _serverRepository.UpdateRole(Context.Guild.Id, DiscordRoleType.Shadow, role.Id).ConfigureAwait(false);
            });

            await _logger.CommandUsedLogAsync("GlobalPrefixModule", "shadowping", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [Command("premium")]
        [Alias("patreon")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Premium()
        {
            Embed premiumEmbed = DiscordEmbedBuilder.BuildPremiumCommandEmbed();
            await Context.Message.ReplyAsync(embed: premiumEmbed).ConfigureAwait(false);
            await _logger.CommandUsedLogAsync("GlobalInteractionModule", "premium", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [Command("help")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task Help()
        {
            Embed helpEmbed = DiscordEmbedBuilder.BuildHelpCommandEmbed();
            await Context.Message.ReplyAsync(embed: helpEmbed).ConfigureAwait(false);
            await _logger.CommandUsedLogAsync("GlobalInteractionModule", "help", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }
    }
}
