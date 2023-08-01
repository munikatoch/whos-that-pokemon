using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosThatPokemon.Model.MachineLearning
{
    public class ModelOutput
    {
        public float[]? Score { get; set; }

        public int PredictedPokemonLabel { get; set; }
    }
}
