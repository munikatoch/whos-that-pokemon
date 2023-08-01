using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosThatPokemon.Interfaces.Config
{
    public interface IAppConfig
    {
        T GetValue<T>(string key, T defaultValue) where T : notnull;
    }
}
