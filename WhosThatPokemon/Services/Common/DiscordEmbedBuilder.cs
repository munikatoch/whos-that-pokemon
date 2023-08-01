using Discord;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using WhosThatPokemon.Model.DataAccess;

namespace WhosThatPokemon.Services.Common
{
    public class DiscordEmbedBuilder
    {
        public static Embed BuildPokemonCollection(List<Pokemon> pokemons)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Title = "Your Pokemon Collection";
            embedBuilder.WithCurrentTimestamp();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < pokemons.Count; i++)
            {
                string pokemonName = TextUtil.ChangeToPascalCase(pokemons[i].PokemonName);
                sb.AppendLine(pokemonName);
            }
            embedBuilder.Description = sb.ToString();
            return embedBuilder.Build();
        }

        public static Task BuildPokemonPredictionModel(EmbedBuilder embedBuilder, Pokemon pokemon)
        {
            string pokemonName = TextUtil.ChangeToPascalCase(pokemon.PokemonName);
            embedBuilder.AddField("Pokemon Name: ", pokemonName);
            List<string> tags = new List<string>();
            if (pokemon.IsRare)
            {
                tags.Add("Rare");
            }
            if (pokemon.IsRegional)
            {
                tags.Add("Regional");
            }
            if (pokemon.IsShadow)
            {
                tags.Add("Shadow");
            }
            if (tags.Count > 0)
            {
                embedBuilder.AddField("Tags:", string.Join(", ", tags));
            }
            return Task.CompletedTask;
        }

        public static Embed BuildHelpCommandEmbed()
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.Title = "All available bot commands are following:";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("The format of command is [command] - [Description] - [alias]");
            sb.AppendLine("[predict] - [Predict pokemon with the given URL (Note: Only use for pokemon bot for accurate results)] - [p]");
            sb.AppendLine("[collection] - [Add, Remove or List your collection (Note: Available command options are [list, add, remove])] - [collect, cl, c]");
            sb.AppendLine("[rareping] - [Set rare ping role] - [rp]");
            sb.AppendLine("[regionalping] - [Set regional ping role] - [rgp]");
            sb.AppendLine("[shadowping] - [Set shadow ping role] - [sp]");
            sb.AppendLine("Note: For role mention only one type of role will be mentioned in the following decreasing precedence Rare, Shadow, Regional");
            embedBuilder.Description = sb.ToString();
            return embedBuilder.Build();
        }

        internal static Embed BuildPremiumCommandEmbed()
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.Title = "Donate to Whos That Pokemon Bot for special benefits!";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("patreon.com/Cornpuff");
            embedBuilder.Description = sb.ToString();
            return embedBuilder.Build();
        }
    }
}
