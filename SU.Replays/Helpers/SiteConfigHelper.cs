using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Components;
using SU.Replays.Configuration;

namespace SU.Replays.Helpers;

/// <summary>
/// Simple helper that provides proxy methods to the config for the site (looks wise).
/// </summary>
public sealed class SiteConfigHelper
{
    private readonly SiteConfiguration _siteConfiguration = new();
    private readonly ILogger<SiteConfigHelper> _logger;

    public SiteConfigHelper(ILogger<SiteConfigHelper> logger, IConfiguration configuration)
    {
        _logger = logger;

        configuration.Bind(SiteConfiguration.Name, _siteConfiguration);

        LoadStylesheets();
    }

    private Dictionary<string, string> _siteStylesheets = new Dictionary<string, string>();

    /// <summary>
    /// Loads the external stylesheets.
    /// </summary>
    private async void LoadStylesheets()
    {
        _logger.LogInformation("Loading {count} external stylesheets...", _siteConfiguration.ExternalStyleSheets.Length);
        var sw = Stopwatch.StartNew();

        _siteStylesheets.Clear();

        _siteStylesheets.EnsureCapacity(_siteConfiguration.ExternalStyleSheets.Length);

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMilliseconds(_siteConfiguration.HttpTimeout);
        if (!string.IsNullOrEmpty(_siteConfiguration.HttpUserAgent))
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_siteConfiguration.HttpUserAgent);
        }

        foreach (var remoteStylesheet in _siteConfiguration.ExternalStyleSheets)
        {
            _logger.LogDebug("Loading external stylesheet {url}...", remoteStylesheet);
            var stylesheet = await httpClient.GetStringAsync(remoteStylesheet);
            _siteStylesheets.Add(remoteStylesheet, stylesheet);

            _logger.LogDebug("Loaded external stylesheet {url}.", remoteStylesheet);
        }

        _logger.LogInformation("External stylesheets loaded in {time}", sw.Elapsed);
    }

    /// <summary>
    /// Returns an external stylesheet by url. Null if the input has no stylesheet attached to it.
    /// </summary>
    public string? GetStylesheet(string url)
    {
        return _siteStylesheets.TryGetValue(url, out var result) ? result : null;
    }

    /// <summary>
    /// Returns all current stored stylesheets.
    /// </summary>
    public string[] GetStylesheetUrls() => _siteStylesheets.Keys.ToArray();

    public string FooterPrefix => _siteConfiguration.FooterPrefix;
    public bool HasFooterPrefix => !string.IsNullOrEmpty(FooterPrefix);

    public string GithubRepository => _siteConfiguration.GithubRepository;
    public bool HasGithubRepository => !string.IsNullOrEmpty(GithubRepository);

    public string PrivacyPolicy => _siteConfiguration.PrivacyPolicy;
    public bool HasPrivacyPolicy => !string.IsNullOrEmpty(PrivacyPolicy);

    /// <summary>
    /// Provides the site footer.
    /// </summary>
    public MarkupString? BuildFooter()
    {
        if (!HasFooterPrefix
            && !HasGithubRepository
            && !HasPrivacyPolicy
           ) return null; // No footer if theres nothing set to display

        var sb = new StringBuilder();
        sb.AppendLine($"{FooterPrefix}");

        if (HasGithubRepository)
        {
            sb.Append($" - <a href=\"{GithubRepository}\">Source Code</a>");
        }

        if (HasPrivacyPolicy)
        {
            sb.Append($" - <a href=\"{PrivacyPolicy}\">Privacy Policy</a>");
        }

        return new MarkupString(sb.ToString());
    }

    public int ItemsPerPage => _siteConfiguration.ItemsPerPage;
    public int MaxPageButtons => _siteConfiguration.MaxPageButtons;
}