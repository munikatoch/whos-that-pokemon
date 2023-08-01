using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WhosThatPokemon.Model.DataAccess
{
    public class DiscordUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        public ulong DiscordUserId { get; set; }

        [BsonElement("is_premium")]
        [BsonRepresentation(BsonType.Boolean)]
        [BsonDefaultValue(false)]
        public bool IsPremiumUser { get; set; }

        [BsonElement("pokemon_id")]
        [BsonDefaultValue(null)]
        public int[]? PokemonCollection { get; set; }

        [BsonElement("is_afk")]
        [BsonRepresentation(BsonType.Boolean)]
        [BsonDefaultValue(false)]
        public bool IsUserAfk { get; set; }
    }
}
