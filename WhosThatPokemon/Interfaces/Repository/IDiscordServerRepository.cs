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
        Task InsertServerAsync(ulong guildId);
        Task DeleteServerAsync(ulong guildId);
        Task UpdateRoleAsync(ulong guildId, DiscordRoleType roleType, ulong roleId);
        Task<DiscordServer> GetServerDataAsync(ulong guildId);
        Task UpdateChannelAsync(ulong guildId, DiscordChannelType channel, ulong channelId);
    }
}
