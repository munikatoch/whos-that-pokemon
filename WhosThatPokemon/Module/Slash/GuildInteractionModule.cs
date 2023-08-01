using Discord;
using Discord.Interactions;
using Serilog.Events;
using System.IO.Compression;
using WhosThatPokemon.Interfaces.Log;
using WhosThatPokemon.Model;
using WhosThatPokemon.Services.Common;

namespace WhosThatPokemon.Module.Slash
{
    public class GuildInteractionModule : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly IAppLogger _logger;

        public GuildInteractionModule(IAppLogger logger)
        {
            _logger = logger;
        }

        [SlashCommand("version", "Current Bot version")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task BotVersion()
        {
            await RespondAsync(Constants.BotVersionMessage);
            await _logger.CommandUsedLogAsync("GuildInteractionModule", "version", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [SlashCommand("totalserver", "Total servers bot is added to")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task TotalServerIsAdded()
        {
            await RespondAsync(string.Format(Constants.BotTotalMemberMessage, Context.Client.Guilds.Count));
            await _logger.CommandUsedLogAsync("GuildInteractionModule", "totalserver", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
        }

        [SlashCommand("getlogs", "Get log files created")]
        [RequireBotPermission(ChannelPermission.AttachFiles)]
        public async Task GetDiscordBotLogs()
        {
            await RespondAsync(Constants.BotGetLogMessage);

            await _logger.CommandUsedLogAsync("GuildInteractionModule", "getlogs", Context.Guild.Id, Context.Channel.Id, Context.User.Id).ConfigureAwait(false);
            _ = Task.Run(async () =>
            {
                try
                {
                    FileUtil.CreateDirectoryIfNotExists(Constants.LogZipfolder);
                    using (FileStream zipToOpen = new FileStream(Constants.LogZipfile, FileMode.Create))
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        foreach (var file in Directory.GetFiles(Constants.Logfolder))
                        {
                            var entryName = Path.GetFileName(file);
                            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                            entry.LastWriteTime = File.GetLastWriteTime(file);
                            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (var stream = entry.Open())
                            {
                                fs.CopyTo(stream);
                            }
                        }
                    }
                    await Context.Channel.SendFileAsync(Constants.LogZipfile);
                }
                catch (Exception ex)
                {
                    await _logger.ExceptionLogAsync("GuildInteractionModule.GetDiscordBotLogs", ex);
                }
                finally
                {
                    FileUtil.DeleteAllFiles(Constants.LogZipfolder);
                }

            });
        }
    }
}
