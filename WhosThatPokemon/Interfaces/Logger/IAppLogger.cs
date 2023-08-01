using Serilog.Events;

namespace WhosThatPokemon.Interfaces.Logger
{
    public interface IAppLogger
    {
        Task ExceptionLogAsync(string source, Exception exception);
        Task FileLogAsync(object message, LogEventLevel level);
        Task CommandUsedLogAsync(string source, string commandUsed, ulong guildId, ulong channelId, ulong userId);
    }
}
