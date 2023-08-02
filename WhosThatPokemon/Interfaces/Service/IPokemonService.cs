using Discord;
using Discord.WebSocket;
using WhosThatPokemon.Model;

namespace WhosThatPokemon.Interfaces.Service
{
    public interface IPokemonService
    {
        Task<DiscordPokemonPrediction> PredictSpawnedPokemon(string url, Color? color);
        Task<Embed> PredictPokemon(string url);
        bool ValidatePokemonSpanMessage(SocketUserMessage message);
        bool ValidateIsShinyPokemonMessage(SocketUserMessage message);
    }
}
