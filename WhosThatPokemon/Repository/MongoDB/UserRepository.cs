using Discord;
using MongoDB.Driver;
using Serilog.Events;
using WhosThatPokemon.Interfaces.Config;
using WhosThatPokemon.Interfaces.Logger;
using WhosThatPokemon.Interfaces.Repository;
using WhosThatPokemon.Model.DataAccess;

namespace WhosThatPokemon.Repository.MongoDB
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<DiscordUser> _collection;
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IAppConfig _config;
        private readonly IAppLogger _logger;

        public UserRepository(MongoClient client, IPokemonRepository pokemonRepository, IAppConfig config, IAppLogger logger)
        {
            _collection = client.GetDatabase("DiscordBot").GetCollection<DiscordUser>("User");
            _pokemonRepository = pokemonRepository;
            _config = config;
            _logger = logger;
        }

        public async Task<List<Pokemon>> UpsertUserPokemonCollection(ulong userId, string collection)
        {
            string[] pokemonNames = collection.Split(",").Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            DiscordUser user = await GetUserByUserId(userId);

            List<Pokemon> pokemons;

            if (user == null)
            {
                pokemons = await InsertUser(pokemonNames, userId);
            }
            else
            {
                pokemons = await UpdateUserCollection(user, pokemonNames, userId);
            }
            return pokemons;
        }

        public async Task<List<Pokemon>> RemoveUserPokemonCollection(ulong userId, string collection)
        {
            string[] pokemonNames = collection.Split(",").Select(x => x.Trim()).ToArray();
            List<Pokemon> pokemons = (await _pokemonRepository.GetPokemonByName(pokemonNames)).ToList();

            if(pokemons.Count > 0)
            {
                try
                {
                    int[] pokemonIds = pokemons.Select(x => x.PokemonId).ToArray();
                    FilterDefinition<DiscordUser> filter = Builders<DiscordUser>.Filter.Eq(u => u.DiscordUserId, userId);

                    UpdateDefinition<DiscordUser> updatedCollection = Builders<DiscordUser>.Update.PullAll(x => x.PokemonCollection, pokemonIds);

                    UpdateResult result = await _collection.UpdateOneAsync(filter, updatedCollection);

                    await _logger.FileLogAsync(result, LogEventLevel.Information).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _logger.ExceptionLogAsync("UserRepository.RemoveUserPokemonCollection", ex).ConfigureAwait(false);
                    throw;
                }
            }
            return pokemons;
        }

        public async Task<DiscordUser> GetUserByUserId(ulong userId)
        {
            try
            {
                FilterDefinition<DiscordUser> filter = Builders<DiscordUser>.Filter.Eq(u => u.DiscordUserId, userId);
                DiscordUser result = await _collection.Find(filter).FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("UserRepository.GetUserByUserId", ex).ConfigureAwait(false);
            }
            return null;
        }

        private async Task<List<Pokemon>> InsertUser(string[] pokemonNames, ulong userId)
        {
            List<Pokemon> pokemons = new List<Pokemon>();
            try
            {
                int nonPremiumUserCollectionLimit = _config.GetValue("UserPokemonCollectionLimit", 0);
                pokemons = (await _pokemonRepository.GetPokemonByName(pokemonNames))
                            .Where(x => !x.IsRare)
                            .Take(nonPremiumUserCollectionLimit)
                            .ToList();

                int[] pokemonIds = pokemons.Select(x => x.PokemonId).ToArray();
                await _collection.InsertOneAsync(new DiscordUser()
                {
                    DiscordUserId = userId,
                    IsPremiumUser = false,
                    IsUserAfk = false,
                    PokemonCollection = pokemonIds
                });
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("UserRepository.InsertUser", ex).ConfigureAwait(false);
            }
            return pokemons;
        }

        private async Task<List<Pokemon>> UpdateUserCollection(DiscordUser user, string[] pokemonNames, ulong userId)
        {
            List<Pokemon> pokemons = await CreatePokemonCollection(user, pokemonNames);

            if (pokemons.Count > 0)
            {
                try
                {
                    int[] pokemonIds = pokemons.Select(x => x.PokemonId).ToArray();
                    FilterDefinition<DiscordUser> userIdFilter = Builders<DiscordUser>.Filter.Eq(u => u.DiscordUserId, userId);

                    UpdateDefinition<DiscordUser> updatedCollection = Builders<DiscordUser>.Update.PushEach(x => x.PokemonCollection, pokemonIds);

                    UpdateResult result = await _collection.UpdateOneAsync(userIdFilter, updatedCollection);
                    await _logger.FileLogAsync(result, LogEventLevel.Information).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _logger.ExceptionLogAsync("UserRepository.UpdateUserCollection", ex).ConfigureAwait(false);
                }
            }
            return pokemons;
        }

        private async Task<List<Pokemon>> CreatePokemonCollection(DiscordUser user, string[] pokemonNames)
        {
            List<Pokemon> pokemonIds = new List<Pokemon>();
            int nonPremiumUserCollectionLimit = _config.GetValue("UserPokemonCollectionLimit", 0);
            if (user != null)
            {
                if (user.IsPremiumUser || nonPremiumUserCollectionLimit == 0)
                {
                    pokemonIds = (await _pokemonRepository.GetPokemonByName(pokemonNames))
                        .Where(x => x != null && user.PokemonCollection != null && !user.PokemonCollection.Contains(x.PokemonId))
                        .ToList();
                }
                else if (user.PokemonCollection != null && user.PokemonCollection.Length < nonPremiumUserCollectionLimit)
                {
                    pokemonIds = (await _pokemonRepository.GetPokemonByName(pokemonNames))
                        .Where(x => x != null && user.PokemonCollection != null && !user.PokemonCollection.Contains(x.PokemonId) && !x.IsRare)
                        .Take(nonPremiumUserCollectionLimit - user.PokemonCollection.Length)
                        .ToList();
                }
            }
            return pokemonIds;
        }

        public async Task<List<DiscordUser>> GetPokemonCollectingUser(int pokemonId)
        {
            try
            {
                var result = await (await _collection.FindAsync(x => x.PokemonCollection != null && x.PokemonCollection.Contains(pokemonId))).ToListAsync();
                return result;
            }
            catch(Exception ex)
            {
                await _logger.ExceptionLogAsync("UserRepository.UpdateUserCollection", ex).ConfigureAwait(false);
            }
            return null;
        }
    }
}
