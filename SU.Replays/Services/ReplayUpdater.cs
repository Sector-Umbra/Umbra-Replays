using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SU.Replays.Configuration;
using SU.Replays.Database;
using SU.Replays.Database.Models;
using SU.Replays.Services.ReplayParticipantsProvider;

namespace SU.Replays.Services;

public class ReplayUpdater : IHostedService, IDisposable
{
    private readonly ILogger<ReplayUpdater> _logger;
    private IServiceScopeFactory _scopeFactory;

    private Timer _timer;
    private FileSystemWatcher _watcher;
    private ReplayConfiguration _configuration = new();

    public static readonly Regex ReplayFileNameRegex = new(@"(\d{4}_\d{2}_\d{2}-\d{2}_\d{2})-round_(\d+)\.zip");

    public ReplayUpdater(ILogger<ReplayUpdater> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        configuration.Bind(ReplayConfiguration.Name, _configuration);
    }

    public async Task UpdateReplays()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var participantsProvider = scope.ServiceProvider.GetRequiredService<IReplayParticipantsProvider>();

        _logger.LogDebug("Checking for outdated replays...");

        var replays = await context.Replays.ToListAsync();
        foreach (var replay in replays)
        {
            if (!File.Exists(replay.FileLocation))
            {
                _logger.LogInformation("Replay {Replay} is missing, removing from database.", replay.FileLocation);
                context.Replays.Remove(replay);
            }
        }

        await context.SaveChangesAsync();

        _logger.LogDebug("Checking for new replays...");

        var replayFiles = Directory.GetFiles(_configuration.ReplayDirectory, "*.zip");
        foreach (var replayFile in replayFiles)
        {
            if (await context.Replays.FindAsync(replayFile) == null)
            {
                _logger.LogInformation("Found new replay {Replay}, adding to database.", replayFile);
                var fileName = Path.GetFileName(replayFile);
                var match = ReplayFileNameRegex.Match(fileName);
                if (!match.Success)
                {
                    _logger.LogWarning("Replay {Replay} has an invalid name, skipping.", replayFile);
                    continue;
                }

                var roundNumber = int.Parse(match.Groups[2].Value);
                var participants = await participantsProvider.GetParticipants(roundNumber);

                await context.Replays.AddAsync(new Replay() {FileLocation = replayFile, Participants = participants});
                _logger.LogInformation("Added replay {Replay} to database.", replayFile);
            }
        }

        await context.SaveChangesAsync();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_configuration.UpdateRate != 0)
        {
            _timer = new Timer(async void (_) =>
            {
                try
                {
                    await UpdateReplays();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to update replays.");
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(_configuration.UpdateRate));
        }

        _watcher = new FileSystemWatcher(_configuration.ReplayDirectory, "*.zip")
        {
            EnableRaisingEvents = true
        };

        _watcher.Created += async (_, _) =>
        {
            try
            {
                await UpdateReplays();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update replays.");
            }
        };

        _watcher.Deleted += async (_, _) =>
        {
            try
            {
                await UpdateReplays();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update replays.");
            }
        };

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        _watcher?.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _watcher?.Dispose();
    }
}