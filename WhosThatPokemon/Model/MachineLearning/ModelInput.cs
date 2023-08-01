using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosThatPokemon.Model.MachineLearning
{
    public class ModelInput
    {
        public byte[]? Image { get; set; }

        public UInt32 LabelAsKey { get; set; }
    }
}
