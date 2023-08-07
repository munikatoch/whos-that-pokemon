using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WhosThatPokemon.Model.DataAccess
{
    public class DiscordServer
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        public ulong ServerId { get; set; }

        [BsonElement("rare_ping_id")]
        [BsonRepresentation(BsonType.Int64)]
        [BsonDefaultValue(0ul)]
        public ulong RarePingId { get; set; }

        [BsonElement("shadow_ping_id")]
        [BsonRepresentation(BsonType.Int64)]
        [BsonDefaultValue(0ul)]
        public ulong ShadowPingId { get; set; }

        [BsonElement("regional_ping_id")]
        [BsonRepresentation(BsonType.Int64)]
        [BsonDefaultValue(0ul)]
        public ulong RegionalPingId { get; set; }

        [BsonElement("starboard_channel_id")]
        [BsonRepresentation(BsonType.Int64)]
        [BsonDefaultValue(0ul)]
        public ulong StarboardChannelId { get; set; }

        [BsonElement("is_premium")]
        [BsonRepresentation(BsonType.Boolean)]
        [BsonDefaultValue(false)]
        public bool IsPremiumServer { get; set; }
    }
}
