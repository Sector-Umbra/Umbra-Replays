using Npgsql;
using SU.Replays.Configuration;

namespace SU.Replays.Services.ReplayParticipantsProvider;

public class ReplayParticipantsProvider : IReplayParticipantsProvider
{
    // TsjipTsjip
    // Assume existence of a player_round_view view which has the user_id column and the round_id column in pairs.
    private const string ParticipantsQuery = @"
        SELECT user_id
        FROM player_round_view
        WHERE round_id = @roundId
    ";

    private ReplayConfiguration _configuration = new();
    private ILogger<ReplayParticipantsProvider> _logger;

    public ReplayParticipantsProvider(IConfiguration configuration, ILogger<ReplayParticipantsProvider> logger)
    {
        _logger = logger;
        configuration.Bind(ReplayConfiguration.Name, _configuration);
    }

    public async Task<List<Guid>> GetParticipants(int roundId)
    {
        if (string.IsNullOrEmpty(_configuration.PostgresConnectionString))
        {
            _logger.LogError("Postgres connection string is not set.");
            throw new InvalidOperationException("Postgres connection string is not set.");
        }

        await using var conn = new NpgsqlConnection(_configuration.PostgresConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(ParticipantsQuery, conn);
        cmd.Parameters.AddWithValue("roundId", roundId);

        await using var reader = await cmd.ExecuteReaderAsync();
        var participants = new List<Guid>();
        while (await reader.ReadAsync())
        {
            participants.Add(reader.GetGuid(0));
        }

        return participants;
    }
}