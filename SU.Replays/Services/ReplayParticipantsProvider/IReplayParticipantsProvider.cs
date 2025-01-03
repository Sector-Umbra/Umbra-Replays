namespace SU.Replays.Services.ReplayParticipantsProvider;

public interface IReplayParticipantsProvider
{
    /// <summary>
    /// Provides the participants of a replay based on the round ID.
    /// </summary>
    Task<List<Guid>> GetParticipants(int roundId);
}