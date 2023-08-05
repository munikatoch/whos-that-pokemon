using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using WhosThatPokemon.Model.DataAccess;
using WhosThatPokemon.Model.Enum;

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
            embedBuilder.AddField("Pokemon Name: ", $"```{pokemonName}```");
            return Task.CompletedTask;
        }

        public static Embed BuildHelpCommandEmbed()
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.Title = "All available bot commands are following:";
            embedBuilder.Description = "Note: For role mention only one type of role will be mentioned in the following decreasing precedence Rare, Shadow, Regional";

            embedBuilder.AddField("[predict, p]", "Predict pokemon with the given URL (Note: Only use for pokemon bot for accurate results)");
            embedBuilder.AddField("[collection, collect, cl, c]", "Add, Remove or List your collection (Note: Available command options are {list, add, remove})");
            embedBuilder.AddField("[rareping, rp]", "Set rare ping role");
            embedBuilder.AddField("[regionalping, rgp]", "Set regional ping role");
            embedBuilder.AddField("[shadowping, sp]", "Set shadow ping role");
            embedBuilder.AddField("[afk]", "Set user afk to stop collection ping");
            embedBuilder.AddField("[starboard, sb]", "Set starboard channel");
            embedBuilder.AddField("[premium, patreon]", "Donate to whos that pokemon bot");

            return embedBuilder.Build();
        }

        public static Embed BuildPremiumCommandEmbed()
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.Title = "Donate to Whos That Pokemon Bot for special benefits!";
            embedBuilder.Description = "https://www.patreon.com/Cornpuff";
            return embedBuilder.Build();
        }

        public static Embed BuildAddedPokemonEmbed(List<Pokemon> addedPokemon, PokemonCollectionOperation operation)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();

            if (operation == PokemonCollectionOperation.Add)
            {
                embedBuilder.Title = "Pokemons added to collection are:";
            }
            else
            {
                embedBuilder.Title = "Pokemons removed from collection are:";
            }
            StringBuilder sb = new StringBuilder();
            foreach (var pokemon in addedPokemon)
            {
                sb.AppendLine(TextUtil.ChangeToPascalCase(pokemon.PokemonName));
            }
            embedBuilder.Description = sb.ToString();
            return embedBuilder.Build();
        }

        public static Embed BuildStartboardEmbed(SocketUserMessage message, string jumpUrl)
        {
            Embed? pokemonEmbed = message.Embeds.FirstOrDefault();
            EmbedBuilder embedBuilder = new EmbedBuilder();
            if (pokemonEmbed != null)
            {
                embedBuilder.Title = pokemonEmbed.Title;
                embedBuilder.Description = pokemonEmbed.Description;
                embedBuilder.ImageUrl = pokemonEmbed.Image.GetValueOrDefault().Url;
                foreach (var pokemonField in pokemonEmbed.Fields)
                {
                    embedBuilder.AddField(pokemonField.Name, pokemonField.Value, pokemonField.Inline);
                }
                embedBuilder.Color = pokemonEmbed.Color;
                embedBuilder.AddField("Go to message: ", $"[Jump to message]({jumpUrl})");
            }
            else
            {
                embedBuilder.Title = "Shiny Caught";
                embedBuilder.Description = message.Content;
                embedBuilder.AddField("", $"[Jump to message]({jumpUrl})");
            }
            return embedBuilder.Build();
        }
    }
}
