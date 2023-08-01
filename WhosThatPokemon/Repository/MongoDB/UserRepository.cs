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

        public async Task UpsertUserPokemonCollection(ulong userId, string collection)
        {
            string[] pokemonNames = collection.Split(",").Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            DiscordUser user = await GetUserByUserId(userId);

            if(user == null)
            {
                await InsertUser(pokemonNames, userId);
            }
            else
            {
                await UpdateUserCollection(user, pokemonNames, userId);
            }
        }

        public async Task RemoveUserPokemonCollection(ulong userId, string collection)
        {
            string[] pokemonNames = collection.Split(",").Select(x => x.Trim()).ToArray();
            int[] pokemonIds = (await _pokemonRepository.GetPokemonByName(pokemonNames)).Select(x => x.PokemonId).ToArray();

            if(pokemonIds.Length > 0)
            {
                try
                {
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

        private async Task InsertUser(string[] pokemonNames, ulong userId)
        {
            try
            {
                int nonPremiumUserCollectionLimit = _config.GetValue("UserPokemonCollectionLimit", 0);
                int[] pokemonIds = (await _pokemonRepository.GetPokemonByName(pokemonNames))
                            .Where(x => !x.IsRegional && !x.IsRare && !x.IsShadow)
                            .Select(x => x.PokemonId)
                            .Take(nonPremiumUserCollectionLimit)
                            .ToArray();
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
        }

        private async Task UpdateUserCollection(DiscordUser user, string[] pokemonNames, ulong userId)
        {
            int[] pokemonIds = await CreatePokemonCollection(user, pokemonNames);

            if (pokemonIds.Length > 0)
            {
                try
                {
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
        }

        private async Task<int[]> CreatePokemonCollection(DiscordUser user, string[] pokemonNames)
        {
            int[] pokemonIds = new int[0];
            int nonPremiumUserCollectionLimit = _config.GetValue("UserPokemonCollectionLimit", 0);
            if (user != null)
            {
                if (user.IsPremiumUser || nonPremiumUserCollectionLimit == 0)
                {
                    pokemonIds = (await _pokemonRepository.GetPokemonByName(pokemonNames))
                        .Where(x => x != null && user.PokemonCollection != null && !user.PokemonCollection.Contains(x.PokemonId) && !x.IsRegional && !x.IsRare && !x.IsShadow)
                        .Select(x => x.PokemonId)
                        .ToArray();
                }
                else if (user.PokemonCollection != null && user.PokemonCollection.Length < nonPremiumUserCollectionLimit)
                {
                    pokemonIds = (await _pokemonRepository.GetPokemonByName(pokemonNames))
                        .Where(x => x != null && user.PokemonCollection != null && !user.PokemonCollection.Contains(x.PokemonId) && !x.IsRegional && !x.IsRare && !x.IsShadow)
                        .Select(x => x.PokemonId)
                        .Take(nonPremiumUserCollectionLimit - user.PokemonCollection.Length)
                        .ToArray();
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
