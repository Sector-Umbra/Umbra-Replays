namespace SU.Replays.Services.ReplayParticipantsProvider;

/// <summary>
/// Provides debug participants for replays.
/// </summary>
public class DebugReplayParticipantsProvider : IReplayParticipantsProvider
{
    /// <inheritdoc />
    public Task<List<Guid>> GetParticipants(int roundId)
    {
        if (roundId == 1)
        {
            return Task.FromResult(new List<Guid>
            {
                new Guid("0f80cf86-fdc6-4fc1-a075-4d872578b3cc"),
            });
        }
        else
        {
            return Task.FromResult(new List<Guid>());
        }
    }
}