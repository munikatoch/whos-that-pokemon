using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosThatPokemon.Model.DataAccess;

namespace WhosThatPokemon.Interfaces.Repository
{
    public interface IPokemonRepository
    {
        Task<List<Pokemon>> GetPokemonByName(string[] pokemonsName);
        Task<List<Pokemon>> GetPokemonById(int[] pokemonsId);
        Task<Pokemon> GetPokemonById(int pokemonsId, bool updateCount);
    }
}
