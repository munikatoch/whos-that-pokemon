﻿using Discord;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Search;
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

        public async Task<List<Pokemon>> UpsertUserPokemonCollectionAsync(ulong userId, string collection)
        {
            string[] pokemonNames = collection.Split(",").Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            DiscordUser user = await GetUserByUserIdAsync(userId);

            List<Pokemon> pokemons;

            if (user == null)
            {
                pokemons = await InsertUserWithCollectionAsync(pokemonNames, userId);
            }
            else
            {
                pokemons = await UpdateUserCollectionAsync(user, pokemonNames);
            }
            return pokemons;
        }

        public async Task<List<Pokemon>> RemoveUserPokemonCollectionAsync(ulong userId, string collection)
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
                    await _logger.ExceptionLogAsync($"UserRepository.RemoveUserPokemonCollection UserId: {userId} Collection: {collection}", ex).ConfigureAwait(false);
                    throw;
                }
            }
            return pokemons;
        }

        public async Task<DiscordUser> GetUserByUserIdAsync(ulong userId)
        {
            try
            {
                FilterDefinition<DiscordUser> filter = Builders<DiscordUser>.Filter.Eq(u => u.DiscordUserId, userId);
                DiscordUser result = await _collection.Find(filter).FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync($"UserRepository.GetUserByUserId UserId: {userId}", ex).ConfigureAwait(false);
            }
            return null;
        }

        public async Task<List<DiscordUser>> GetPokemonCollectingUserAsync(int pokemonId)
        {
            try
            {
                var equalOperator = new Dictionary<string, object>
                {
                    { "value", pokemonId },
                    { "path", "pokemon_id" }
                };

                var searchOperator = new Dictionary<string, object>
                {
                    { "index", "PokemonCollectionIdx" },
                    { "equals", equalOperator },
                    { "returnStoredSource", true }
                };

                BsonDocument element = new BsonDocument(searchOperator);

                var filter = new BsonDocumentSearchDefinition<DiscordUser>(element);

                var result = _collection.Aggregate().Search(filter).ToList();
                return result;
            }
            catch(Exception ex)
            {
                await _logger.ExceptionLogAsync($"UserRepository.GetPokemonCollectingUser PokemonId : {pokemonId}", ex).ConfigureAwait(false);
            }
            return null;
        }

        public async Task UpdateUserAfkStatusAsync(DiscordUser user)
        {
            try
            {
                FilterDefinition<DiscordUser> filter = Builders<DiscordUser>.Filter.Eq(x => x.DiscordUserId, user.DiscordUserId);
                UpdateDefinition<DiscordUser> update = Builders<DiscordUser>.Update.Set(x => x.IsUserAfk, !user.IsUserAfk);
                UpdateResult result = await _collection.UpdateOneAsync(filter, update);
                await _logger.FileLogAsync(result, LogEventLevel.Information).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync($"UserRepository.UpdateUserAfkStatusAsync User ID: {user.DiscordUserId} Afk Status: {!user.IsUserAfk}", ex).ConfigureAwait(false);
            }
        }

        public async Task InsertUserAsync(DiscordUser user)
        {
            try
            {
                await _collection.InsertOneAsync(user);
            }
            catch(Exception ex)
            {
                await _logger.ExceptionLogAsync($"UserRepository.InsertUserAsync User ID: {user.DiscordUserId}", ex).ConfigureAwait(false);
            }
        }

        private async Task<List<Pokemon>> CreatePokemonCollectionAsync(DiscordUser user, string[] pokemonNames)
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

        private async Task<List<Pokemon>> UpdateUserCollectionAsync(DiscordUser user, string[] pokemonNames)
        {
            List<Pokemon> pokemons = await CreatePokemonCollectionAsync(user, pokemonNames);

            if (pokemons.Count > 0)
            {
                try
                {
                    int[] pokemonIds = pokemons.Select(x => x.PokemonId).ToArray();
                    FilterDefinition<DiscordUser> userIdFilter = Builders<DiscordUser>.Filter.Eq(u => u.DiscordUserId, user.DiscordUserId);

                    UpdateDefinition<DiscordUser> updatedCollection = Builders<DiscordUser>.Update.PushEach(x => x.PokemonCollection, pokemonIds);

                    UpdateResult result = await _collection.UpdateOneAsync(userIdFilter, updatedCollection);
                    await _logger.FileLogAsync(result, LogEventLevel.Information).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _logger.ExceptionLogAsync($"UserRepository.UpdateUserCollection PokemonNames: {pokemonNames} UserId: {user.DiscordUserId}", ex).ConfigureAwait(false);
                }
            }
            return pokemons;
        }

        private async Task<List<Pokemon>> InsertUserWithCollectionAsync(string[] pokemonNames, ulong userId)
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
                await _logger.ExceptionLogAsync($"UserRepository.InsertUserWithCollection PokemonNames: {pokemonNames} UserId: {userId}", ex).ConfigureAwait(false);
            }
            return pokemons;
        }
    }
}
