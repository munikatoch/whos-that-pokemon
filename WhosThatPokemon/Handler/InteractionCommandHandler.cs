using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog.Events;
using WhosThatPokemon.Interfaces.Config;
using WhosThatPokemon.Interfaces.Discord;
using WhosThatPokemon.Interfaces.Logger;
using WhosThatPokemon.Module.Slash;

namespace WhosThatPokemon.Handler
{
    public class InteractionCommandHandler : IInteractionHandler
    {
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppLogger _logger;
        private readonly IAppConfig _appConfig;

        public InteractionCommandHandler(DiscordShardedClient client, InteractionService interactionService, IServiceProvider serviceProvider, IAppLogger logger, IAppConfig config)
        {
            _client = client;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _appConfig = config;
        }

        public async Task InitializeAsync()
        {
            ulong guildId = _appConfig.GetValue("PrimaryGuildId", 0ul);

            ModuleInfo globalModule = await _interactionService.AddModuleAsync<GlobalInteractionModule>(_serviceProvider);
            ModuleInfo guildModule = await _interactionService.AddModuleAsync<GuildInteractionModule>(_serviceProvider);

            _client.ShardReady += async (x) =>
            {
                await _interactionService.AddModulesGloballyAsync(true, new ModuleInfo[] { globalModule });
                await _interactionService.AddModulesToGuildAsync(guildId, true, new ModuleInfo[] { guildModule });
            };

            _interactionService.Log += LogInteractionServiceEvent;
            _client.SlashCommandExecuted += SlashCommandExecutedEvent;
        }

        private async Task LogInteractionServiceEvent(LogMessage log)
        {
            Enum.TryParse(log.Severity.ToString(), out LogEventLevel logLevel);
            await _logger.FileLogAsync(log, logLevel).ConfigureAwait(false);
        }

        private async Task SlashCommandExecutedEvent(SocketSlashCommand command)
        {
            ShardedInteractionContext context = new ShardedInteractionContext(_client, command);
            IResult result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
            if (!result.IsSuccess)
            {
                await _logger.FileLogAsync(result, LogEventLevel.Error).ConfigureAwait(false);
                await command.RespondAsync(result.ErrorReason);
            }
        }
    }
}
