using MongoDB.Driver;
using Serilog.Events;
using WhosThatPokemon.Interfaces.Logger;
using WhosThatPokemon.Interfaces.Repository;
using WhosThatPokemon.Model;
using WhosThatPokemon.Model.DataAccess;
using WhosThatPokemon.Model.Enum;

namespace WhosThatPokemon.Repository.MongoDB
{
    public class DiscordServerRepository : IDiscordServerRepository
    {
        private readonly IMongoCollection<DiscordServer> _collection;
        private readonly IAppLogger _logger;

        public DiscordServerRepository(MongoClient client, IAppLogger logger)
        {
            _logger = logger;
            _collection = client.GetDatabase("DiscordBot").GetCollection<DiscordServer>("Server");
        }

        public async Task InsertServerAsync(ulong guildId)
        {
            try
            {
                await _collection.InsertOneAsync(new DiscordServer()
                {
                    ServerId = guildId,
                });

                await _logger.FileLogAsync(new
                {
                    InsertedServerId = guildId,
                    Message = Constants.DatabaseInsertServerMessage
                }, LogEventLevel.Information).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("DiscordServerRepository.InsertServerAsync", ex).ConfigureAwait(false);
            }
        }

        public async Task DeleteServerAsync(ulong guildId)
        {
            try
            {
                FilterDefinition<DiscordServer> filter = Builders<DiscordServer>.Filter.Eq(server => server.ServerId, guildId);
                DeleteResult deleteResult = await _collection.DeleteOneAsync(filter);
                await _logger.FileLogAsync(new
                {
                    DeletedServerId = guildId,
                    DeleteResult = deleteResult,
                    Message = string.Format(Constants.DatabaseDeleteServerMessage, $"DeleteServerAsync {guildId}")
                }, LogEventLevel.Information).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("DiscordServerRepository.DeleteServerAsync", ex).ConfigureAwait(false);
            }
        }

        public async Task UpdateRoleAsync(ulong guildId, DiscordRoleType roleType, ulong roleId)
        {
            try
            {
                FilterDefinition<DiscordServer> filter = Builders<DiscordServer>.Filter.Eq(x => x.ServerId, guildId);
                UpdateDefinition<DiscordServer> updateDefinition;
                if (roleType == DiscordRoleType.Rare)
                {
                    updateDefinition = Builders<DiscordServer>.Update.Set(x => x.RarePingId, roleId);
                }
                else if (roleType == DiscordRoleType.Regional)
                {
                    updateDefinition = Builders<DiscordServer>.Update.Set(x => x.RegionalPingId, roleId);
                }
                else
                {
                    updateDefinition = Builders<DiscordServer>.Update.Set(x => x.ShadowPingId, roleId);
                }

                UpdateResult updateResult = await _collection.UpdateOneAsync(filter, updateDefinition);

                await _logger.FileLogAsync(new
                {
                    DeletedServerId = guildId,
                    UpdateResult = updateResult,
                    Message = string.Format(Constants.DatabaseDeleteServerMessage, $"UpdateRole {roleType}")
                }, LogEventLevel.Information).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("DiscordServerRepository.UpdateRole", ex).ConfigureAwait(false);
            }
        }

        public async Task<DiscordServer> GetServerDataAsync(ulong guildId)
        {
            try
            {
                DiscordServer server = await _collection.Find(x => x.ServerId == guildId).FirstOrDefaultAsync();
                return server;
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("DiscordServerRepository.GetMentionRoles", ex).ConfigureAwait(false);
            }
            return null;
        }

        public async Task UpdateChannelAsync(ulong guildId, DiscordChannelType channelType, ulong channelId)
        {
            try
            {
                FilterDefinition<DiscordServer> filter = Builders<DiscordServer>.Filter.Eq(x => x.ServerId, guildId);
                UpdateDefinition<DiscordServer> updateDefinition;
                updateDefinition = Builders<DiscordServer>.Update.Set(x => x.StarboardChannelId, channelId);
                UpdateResult updateResult = await _collection.UpdateOneAsync(filter, updateDefinition);

                await _logger.FileLogAsync(new
                {
                    DeletedServerId = guildId,
                    UpdateResult = updateResult,
                    Message = string.Format(Constants.DatabaseUpdateServerMessage, $"UpdateChannelAsync {channelType}")
                }, LogEventLevel.Information).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("DiscordServerRepository.UpdateChannelAsync", ex).ConfigureAwait(false);
            }
        }
    }
}
