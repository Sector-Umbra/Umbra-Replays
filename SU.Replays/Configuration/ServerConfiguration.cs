namespace SU.Replays.Configuration;

public class ServerConfiguration
{
    public const string Name = "Server";

    public Uri Host { get; set; } = new("https://localhost:7154");
    public List<string> CorsOrigins { get; set; } = default!;

    /// <summary>
    /// Enables https redirection if true. Set this to false if run behind a reverse proxy
    /// </summary>
    public bool UseHttps { get; set; } = false;

    /// <summary>
    /// Enables support for reverse proxy headers like "X-Forwarded-Host" if true. Set this to true if run behind a reverse proxy
    /// </summary>
    public bool UseForwardedHeaders { get; set; } = true;

    /// <summary>
    /// Sets the request base path used before any routes apply i.e. "/base/api/Maps" with "/base" being the PathBase. <br/>
    /// Set this if run behind a reverse proxy on a sub path and the proxy doesn't strip the path the server is hosted on.
    /// </summary>
    /// <remarks>
    /// Add a slash before the path: "/path"
    /// </remarks>
    public string? PathBase { get; set; }

    /// <summary>
    /// The SQLite connection string for the database.
    /// </summary>
    public string ConnectionString { get; set; } = default!;

    /// <summary>
    /// This is the address that game servers will connect to.
    /// </summary>
    public string OicdAuthority { get; set; } = default!;
    /// <summary>
    /// The client id for the OICD server.
    /// </summary>
    public string OicdClientId { get; set; } = default!;

    /// <summary>
    /// The client secret for the OICD server.
    /// </summary>
    public string OicdClientSecret { get; set; } = default!;
}