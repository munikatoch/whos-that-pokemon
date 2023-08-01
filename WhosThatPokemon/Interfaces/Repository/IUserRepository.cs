using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosThatPokemon.Model.DataAccess;

namespace WhosThatPokemon.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task<List<Pokemon>> UpsertUserPokemonCollection(ulong userId, string collection);
        Task<List<Pokemon>> RemoveUserPokemonCollection(ulong userId, string collection);
        Task<DiscordUser> GetUserByUserId(ulong userId);
        Task<List<DiscordUser>> GetPokemonCollectingUser(int pokemonId);
    }
}
