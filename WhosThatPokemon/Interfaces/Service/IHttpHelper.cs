using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosThatPokemon.Interfaces.Service
{
    public interface IHttpHelper
    {
        Task<byte[]?> GetImageContent(string url, string type);
    }
}
