using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosThatPokemon.Model.DataAccess;

namespace WhosThatPokemon.Model
{
    public class DiscordPokemonPrediction
    {
        public Embed? PokemonEmbed { get; set; }
        public Pokemon? Pokemon { get; set; }
    }
}
