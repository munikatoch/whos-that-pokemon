using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using WhosThatPokemon.Interfaces.Logger;

namespace WhosThatPokemon.Logging
{
    public class FileLogger : IAppLogger
    {
        private readonly ILogger _logger;
        public FileLogger(ILogger logger)
        {
            _logger = logger;
        }

        public async Task CommandUsedLogAsync(string source, string commandUsed, ulong guildId, ulong channelId, ulong userId)
        {
            await FileLogAsync(new
            {
                Source = source,
                CommandUsed = commandUsed,
                CommandGuild = guildId,
                CommandChannel = channelId,
                CommandUser = userId
            }, LogEventLevel.Information);
        }

        public Task ExceptionLogAsync(string source, Exception exception)
        {
            _logger.Error(exception, source);
            return Task.CompletedTask;
        }

        public Task FileLogAsync(object message, LogEventLevel level)
        {
            string jsonLogMessage = JsonConvert.SerializeObject(message);
            _logger.Write(level, jsonLogMessage);
            return Task.CompletedTask;
        }
    }
}
