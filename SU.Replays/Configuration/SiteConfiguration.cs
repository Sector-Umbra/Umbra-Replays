namespace SU.Replays.Configuration;

/// <summary>
/// Contains settings for how the site will be displayed.
/// </summary>
public class SiteConfiguration
{
    public const string Name = "Site";

    /// <summary>
    /// Anything in this will get put before the links.
    /// </summary>
    public string FooterPrefix { get; set; } = "© Sector Umbra 2025";

    /// <summary>
    /// The link to the privacy policy. If empty the field will be omitted in the footer.
    /// </summary>
    public string PrivacyPolicy { get; set; } = string.Empty;

    /// <summary>
    /// The link to the GitHub repository. If empty the field will be omitted in the footer.
    /// </summary>
    public string GithubRepository { get; set; } = "https://github.com/Sector-Umbra/Umbra-Replays";

    /// <summary>
    /// Style sheets that will be loaded by the site. Cached on the server once it's started. To refresh, restart the server.
    /// </summary>
    public string[] ExternalStyleSheets { get; set; } = [];

    /// <summary>
    /// The http user agent to use when fetching the stylesheets.
    /// </summary>
    public string HttpUserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Timeout in seconds for the http client.
    /// </summary>
    public int HttpTimeout { get; set; } = 5000;
}