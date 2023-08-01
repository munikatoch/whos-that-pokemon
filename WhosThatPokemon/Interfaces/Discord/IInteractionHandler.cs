using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosThatPokemon.Interfaces.Discord
{
    public interface IInteractionHandler
    {
        Task InitializeAsync();
    }
}
