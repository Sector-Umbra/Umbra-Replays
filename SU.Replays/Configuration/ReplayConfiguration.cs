namespace SU.Replays.Configuration;

public class ReplayConfiguration
{
    public const string Name = "Replay";

    /// <summary>
    /// The directory where replays are stored.
    /// </summary>
    public string ReplayDirectory { get; set; } = default!;

    /// <summary>
    /// How often the program checks for new replays in seconds. Set to 0 to disable. It will still check when a file is added using FileSystemWatcher.
    /// </summary>
    public int UpdateRate { get; set; } = 60;

    /// <summary>
    /// The postgres connection string to connect to the SS14 database to get participant data.
    /// </summary>
    public string PostgresConnectionString { get; set; } = default!;
}