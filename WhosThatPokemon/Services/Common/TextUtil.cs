using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhosThatPokemon.Model.DataAccess;

namespace WhosThatPokemon.Services.Common
{
    public class TextUtil
    {
        public static string ChangeToPascalCase(string stringToChange)
        {
            stringToChange = stringToChange.ToLower();
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo; ;
            string updatedString = textInfo.ToTitleCase(stringToChange);
            return updatedString;
        }
    }
}
