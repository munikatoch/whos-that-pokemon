using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WhosThatPokemon.Model.DataAccess
{
    public class Pokemon
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int32)]
        public int PokemonId { get; set; } = 0;

        [BsonElement("pokemon_name")]
        [BsonRepresentation(BsonType.String)]
        public string PokemonName { get; set; } = string.Empty;

        [BsonElement("pokemon_altname")]
        [BsonDefaultValue(null)]
        public string[]? AlternatePokemonName { get; set; }

        [BsonElement("pokemon_type")]
        [BsonDefaultValue(null)]
        public string[]? PokemonTypes { get; set; }

        [BsonElement("is_shadow")]
        [BsonRepresentation(BsonType.Boolean)]
        [BsonDefaultValue(false)]
        public bool IsShadow { get; set; }

        [BsonElement("is_rare")]
        [BsonRepresentation(BsonType.Boolean)]
        [BsonDefaultValue(false)]
        public bool IsRare { get; set; }

        [BsonElement("is_regional")]
        [BsonRepresentation(BsonType.Boolean)]
        [BsonDefaultValue(false)]
        public bool IsRegional { get; set; }
    }
}
