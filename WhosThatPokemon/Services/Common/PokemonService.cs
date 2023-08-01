using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using WhosThatPokemon.Interfaces.Log;
using WhosThatPokemon.Interfaces.Repository;
using WhosThatPokemon.Interfaces.Service;
using WhosThatPokemon.Model;
using WhosThatPokemon.Model.DataAccess;
using WhosThatPokemon.Model.Enum;
using WhosThatPokemon.Model.MachineLearning;

namespace WhosThatPokemon.Services.Common
{
    public class PokemonService : IPokemonService
    {
        private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEnginePool;
        private readonly IHttpHelper _httpHelper;
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IAppLogger _logger;

        public PokemonService(PredictionEnginePool<ModelInput, ModelOutput> predictionEnginePool, IHttpHelper httpHelper, IPokemonRepository pokemonRepository, IAppLogger logger)
        {
            _predictionEnginePool = predictionEnginePool;
            _httpHelper = httpHelper;
            _pokemonRepository = pokemonRepository;
            _logger = logger;
        }

        public async Task<Embed> PredictPokemon(string url)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Title = "Pokemon Prediction";
            Pokemon? pokemon = await GetPokemonImageAndPredictPokemon(url);
            if(pokemon != null)
            {
                await DiscordEmbedBuilder.BuildPokemonPredictionModel(embedBuilder, pokemon);
            }
            return embedBuilder.Build();
        }

        public async Task<DiscordPokemonPrediction> PredictSpawnedPokemon(string url, Color? color)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Title = "Pokemon Prediction";
            embedBuilder.Color = color;
            Pokemon? pokemon = await GetPokemonImageAndPredictPokemon(url);
            if (pokemon != null)
            {
                await DiscordEmbedBuilder.BuildPokemonPredictionModel(embedBuilder, pokemon);
            }
            return new DiscordPokemonPrediction()
            {
                Pokemon = pokemon,
                PokemonEmbed = embedBuilder.Build()
            };
        }

        private async Task<Pokemon?> GetPokemonImageAndPredictPokemon(string url)
        {
            byte[]? imageContent = await _httpHelper.GetImageContent(url, HttpClientType.Pokemon.ToString());
            if (imageContent != null && imageContent.Length > 0)
            {
                PredictionEngine<ModelInput, ModelOutput> predictionEngine = _predictionEnginePool.GetPredictionEngine();
                ModelInput imageToPredict = new ModelInput
                {
                    Image = imageContent
                };
                ModelOutput prediction = predictionEngine.Predict(imageToPredict);
                if(prediction.PredictedPokemonLabel == 0)
                {
                    return null;
                }
                Pokemon pokemon = await _pokemonRepository.GetPokemonById(prediction.PredictedPokemonLabel);
                if (prediction?.Score != null && prediction.Score.Max() * 100 < 50)
                {
                    await _logger.FileLogAsync(new { Url = url, PredictedName = pokemon.PokemonName, Score = prediction.Score.Max() * 100 }, Serilog.Events.LogEventLevel.Error).ConfigureAwait(false);
                    return null;
                }
                return pokemon;
            }
            return null;
        }

        public bool ValidatePokemonSpanMessage(SocketUserMessage message)
        {
            if (message.Author.Id != Constants.PokemonBotAuthorId)
            {
                return false;
            }
            Embed? embed = message.Embeds.FirstOrDefault();
            if(embed == null || string.IsNullOrEmpty(embed.Title))
            {
                return false;
            }
            return embed.Title.Equals("A wild pokémon has аppeаred!", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
