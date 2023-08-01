using MongoDB.Driver;
using WhosThatPokemon.Interfaces.Logger;
using WhosThatPokemon.Interfaces.Repository;
using WhosThatPokemon.Model.DataAccess;

namespace WhosThatPokemon.Repository.MongoDB
{
    public class PokemonRepository : IPokemonRepository
    {
        private readonly IMongoCollection<Pokemon> _collection;
        private readonly IAppLogger _logger;

        public PokemonRepository(MongoClient client, IAppLogger logger)
        {
            _collection = client.GetDatabase("DiscordBot").GetCollection<Pokemon>("Pokemon");
            _logger = logger;
        }

        public async Task<List<Pokemon>> GetPokemonByName(string[] pokemonsName)
        {
            try
            {
                pokemonsName = pokemonsName.Select(x => x.ToLower()).ToArray();
                FilterDefinition<Pokemon> filter = Builders<Pokemon>.Filter.In(r => r.PokemonName, pokemonsName);
                List<Pokemon> result = await _collection.Find(filter).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("PokemonRepository.GetPokemonByName", ex).ConfigureAwait(false);
            }
            return null;
        }

        public async Task<List<Pokemon>> GetPokemonById(int[] pokemonsId)
        {
            try
            {
                FilterDefinition<Pokemon> filter = Builders<Pokemon>.Filter.In(r => r.PokemonId, pokemonsId);
                List<Pokemon> result = await _collection.Find(filter).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("PokemonRepository.GetPokemonByIds", ex).ConfigureAwait(false);
            }
            return null;
        }

        public async Task<Pokemon> GetPokemonById(int pokemonsId, bool updateCount)
        {
            try
            {
                FilterDefinition<Pokemon> filter = Builders<Pokemon>.Filter.Eq(r => r.PokemonId, pokemonsId);
                UpdateDefinition<Pokemon> update = Builders<Pokemon>.Update.Inc(r => r.SpawnCount, updateCount ? 1 : 0);
                Pokemon result = await _collection.FindOneAndUpdateAsync(filter, update);
                return result;
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("PokemonRepository.GetPokemonById", ex).ConfigureAwait(false);
            }
            return null;
        }
    }
}
