using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosThatPokemon.Model.DataAccess;
using WhosThatPokemon.Model.Enum;

namespace WhosThatPokemon.Interfaces.Repository
{
    public interface IDiscordServerRepository
    {
        Task InsertServerAsync(ulong id);
        Task DeleteServerAsync(ulong id);
        Task UpdateRole(ulong id, DiscordRoleType roleType, ulong roleId);
        Task<DiscordServer> GetMentionRoles(ulong id);
    }
}
