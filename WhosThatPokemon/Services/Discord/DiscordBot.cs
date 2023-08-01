using Discord;
using Discord.WebSocket;
using Serilog.Events;
using WhosThatPokemon.Interfaces.Config;
using WhosThatPokemon.Interfaces.Discord;
using WhosThatPokemon.Interfaces.Logger;
using WhosThatPokemon.Interfaces.Repository;

namespace WhosThatPokemon.Services.Discord
{
    public class DiscordBot : IDiscordBot
    {

        private readonly DiscordShardedClient _client;
        private readonly IAppConfig _appConfig;
        private readonly IDiscordServerRepository _discordServerRepository;
        private readonly IAppLogger _logger;
        private readonly IInteractionHandler _interactionHandler;
        private readonly IPrefixHandler _prefixHandler;
        private readonly List<ulong> _exemptedServers;

        public DiscordBot(DiscordShardedClient client, IAppConfig appConfig, IDiscordServerRepository discordServerRepository, IAppLogger logger, IInteractionHandler interactionHandler, IPrefixHandler prefixHandler)
        {
            _client = client;
            _appConfig = appConfig;
            _discordServerRepository = discordServerRepository;
            _logger = logger;
            _exemptedServers = new List<ulong>()
            {
                1037542119319015424,
                1084476576843976754
            };
            _interactionHandler = interactionHandler;
            _prefixHandler = prefixHandler;
        }

        public async Task ConnectAndStartBot()
        {
            string discordBotToken = _appConfig.GetValue("DiscordBotToken", string.Empty);

            await InitializeDiscordClientLogging();

            await _interactionHandler.InitializeAsync();
            await _prefixHandler.InitializeAsync();

            await _client.LoginAsync(TokenType.Bot, discordBotToken);
            await _client.StartAsync();

            Console.WriteLine("Bot online", ConsoleColor.Green);

            await Task.Delay(Timeout.Infinite);
        }

        private Task InitializeDiscordClientLogging()
        {
            _client.Log += LogEventAsync;
            _client.ShardReady += ShardReadyEventAsync;
            _client.ShardConnected += ShardConnectedEventAsync;
            _client.ShardDisconnected += ShardDisconnectedEventAsync;
            _client.ShardLatencyUpdated += ShardLatencyUpdatedEventAsync;
            _client.JoinedGuild += JoinedGuildEventAsync;
            _client.LeftGuild += LeftGuildEventAsync;
            return Task.CompletedTask;
        }

        private async Task LeftGuildEventAsync(SocketGuild guild)
        {
            await _discordServerRepository.DeleteServerAsync(guild.Id).ConfigureAwait(false);
        }

        private async Task JoinedGuildEventAsync(SocketGuild guild)
        {
            int minimumMembersCount = _appConfig.GetValue("MinimumGuildMembers", 0);
            if (minimumMembersCount != 0 && guild.MemberCount < minimumMembersCount && !_exemptedServers.Contains(guild.Id))
            {
                await guild.LeaveAsync().ConfigureAwait(false);
            }
            else
            {
                await _discordServerRepository.InsertServerAsync(guild.Id).ConfigureAwait(false);
            }
        }

        private async Task ShardLatencyUpdatedEventAsync(int oldPing, int newPing, DiscordSocketClient client)
        {
            if(newPing > 300)
            {
                await SendMessageToChannelAsync($"Shard Ping changed ShardId: {client.ShardId} OldLatency: {oldPing} NewLatency: {newPing}").ConfigureAwait(false);
            }
        }

        private async Task ShardDisconnectedEventAsync(Exception exception, DiscordSocketClient client)
        {
            await SendMessageToChannelAsync($"Shard Disconnected ShardID: {client.ShardId} ShardLatency: {client.Latency}").ConfigureAwait(false);
            await _logger.ExceptionLogAsync("DiscordBot.ShardDisconnectedEvent", exception).ConfigureAwait(false);
        }

        private async Task ShardConnectedEventAsync(DiscordSocketClient client)
        {
            await SendMessageToChannelAsync($"Shard Connected ShardID: {client.ShardId} ShardLatency: {client.Latency}").ConfigureAwait(false);
        }

        private Task ShardReadyEventAsync(DiscordSocketClient client)
        {
            Console.WriteLine($"Shard is ready ShardID: {client.ShardId} ShardLatency: {client.Latency}", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        private async Task LogEventAsync(LogMessage log)
        {
            Enum.TryParse(log.Severity.ToString(), out LogEventLevel logLevel);
            await _logger.FileLogAsync(log, logLevel).ConfigureAwait(false);
        }

        private async Task SendMessageToChannelAsync(string message)
        {
            ulong guildId = _appConfig.GetValue("PrimaryGuildId", 0ul);
            ulong channelId = _appConfig.GetValue("PrimaryChannelId", 0ul);

            var guild = _client.GetGuild(guildId);
            if (guild != null)
            {
                var channel = guild.GetTextChannel(channelId);
                if (channel != null)
                {
                    await channel.SendMessageAsync(message);
                }
            }
        }
    }
}
