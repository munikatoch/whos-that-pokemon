using MongoDB.Driver;
using WhosThatPokemon.Interfaces.Log;
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

        public async Task<Pokemon> GetPokemonById(int pokemonsId)
        {
            try
            {
                FilterDefinition<Pokemon> filter = Builders<Pokemon>.Filter.Eq(r => r.PokemonId, pokemonsId);
                Pokemon result = await _collection.Find(filter).FirstOrDefaultAsync();
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
