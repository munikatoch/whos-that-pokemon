using MongoDB.Driver;
using Serilog.Events;
using System.Reflection.Metadata;
using WhosThatPokemon.Interfaces.Log;
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

        public async Task InsertServerAsync(ulong id)
        {
            try
            {
                await _collection.InsertOneAsync(new DiscordServer()
                {
                    ServerId = id,
                });

                await _logger.FileLogAsync(new 
                { 
                    InsertedServerId = id, 
                    Message = Constants.DatabaseInsertServerMessage 
                }, LogEventLevel.Information).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("DiscordServerRepository.InsertServerAsync", ex).ConfigureAwait(false);
            }
        }

        public async Task DeleteServerAsync(ulong id)
        {
            try
            {
                FilterDefinition<DiscordServer> filter = Builders<DiscordServer>.Filter.Eq(server => server.ServerId, id);
                DeleteResult deleteResult = await _collection.DeleteOneAsync(filter);
                await _logger.FileLogAsync(new 
                { 
                    DeletedServerId = id, 
                    deleteResult.DeletedCount, 
                    Message = Constants.DatabaseDeleteServerMessage 
                }, LogEventLevel.Information).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _logger.ExceptionLogAsync("DiscordServerRepository.DeleteServerAsync", ex).ConfigureAwait(false);
            }
        }

        public async Task UpdateRole(ulong id, DiscordRoleType roleType, ulong roleId)
        {
            try
            {
                FilterDefinition<DiscordServer> filter = Builders<DiscordServer>.Filter.Eq(x => x.ServerId, id);
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

                await _collection.UpdateOneAsync(filter, updateDefinition);
            }
            catch(Exception ex)
            {
                await _logger.ExceptionLogAsync("DiscordServerRepository.UpdateRole", ex).ConfigureAwait(false);
            }
        }

        public async Task<DiscordServer> GetMentionRoles(ulong id)
        {
            try
            {
                DiscordServer server = await _collection.Find(x => x.ServerId == id).FirstOrDefaultAsync();
                return server;
            }
            catch(Exception ex)
            {
                await _logger.ExceptionLogAsync("DiscordServerRepository.GetMentionRoles", ex).ConfigureAwait(false);
            }
            return null;
        }
    }
}
