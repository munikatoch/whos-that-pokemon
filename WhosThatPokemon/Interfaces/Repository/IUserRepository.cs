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
        Task<List<Pokemon>> UpsertUserPokemonCollectionAsync(ulong userId, string collection);
        Task<List<Pokemon>> RemoveUserPokemonCollectionAsync(ulong userId, string collection);
        Task<DiscordUser> GetUserByUserIdAsync(ulong userId);
        Task<List<DiscordUser>> GetPokemonCollectingUserAsync(int pokemonId);
        Task UpdateUserAfkStatusAsync(DiscordUser user);
        Task InsertUserAsync(DiscordUser user);
    }
}
