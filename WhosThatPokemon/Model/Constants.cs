using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosThatPokemon.Model
{
    public class Constants
    {
        public static readonly string ProjectRootDirectory = AppContext.BaseDirectory;
        public static readonly string Logfolder = Path.Combine(ProjectRootDirectory, "Log");
        public static readonly string Logfile = Path.Combine(Logfolder, "log.txt");
        public static readonly string LogZipfolder = Path.Combine(ProjectRootDirectory, "Zip");
        public static readonly string LogZipfile = Path.Combine(LogZipfolder, "logs.zip");

        public static readonly string MlModelOutputFileName = "trainedmodel.zip";
        public static readonly string MlModelFileOutputPath = Path.Combine("Assets", MlModelOutputFileName);

        public static readonly string BotVersion = "2.2";
        public static readonly ulong PokemonBotAuthorId = 669228505128501258;

        public static readonly string BotVersionMessage = $"Bot version: {BotVersion}";
        public static readonly string BotTotalMemberMessage = "Total Number servers: {0}";
        public static readonly string BotGetLogMessage = "Here are the logs";
        public static readonly string BotUserAddPokemonCollectionnMessage = "Adding pokemons to collection. If you are not a premium user then only {0} pokemons will be added. Please use 'collection list' to check your collection list";
        public static readonly string BotUserDeletePokemonCollectionnMessage = "Pokemons Removed Successfully";
        public static readonly string BotSetRarePingMessage = "Rare ping set for role {0}";
        public static readonly string BotSetRegionalPingMessage = "Regional ping set for role {0}";
        public static readonly string BotSetShadowPingMessage = "Shadow ping set for role {0}";
        public static readonly string BotListCollectionMessage = "Fetching your Collection";
        public static readonly string BotPredictPokemonMessage = "This will accurately predict pokemons from pokemon bot only";


        public static readonly string DatabaseDeleteServerMessage = "Server Deleted Successfully";
        public static readonly string DatabaseInsertServerMessage = "Server Inserted Successfully";
    }
}
