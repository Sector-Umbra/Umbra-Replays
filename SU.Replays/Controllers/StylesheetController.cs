using Microsoft.AspNetCore.Mvc;
using SU.Replays.Helpers;

namespace SU.Replays.Controllers;

/// <summary>
/// Handles external stylesheets.
/// </summary>
public class StylesheetController : Controller
{
    private readonly SiteConfigHelper _siteConfigHelper;

    public StylesheetController(SiteConfigHelper siteConfigHelper)
    {
        _siteConfigHelper = siteConfigHelper;
    }

    [HttpGet("stylesheet")]
    public IActionResult GetStylesheet(
        [FromQuery] string url
        )
    {
        var stylesheet = _siteConfigHelper.GetStylesheet(url);

        if (stylesheet == null)
            return NotFound();

        var eTag = $"\"{stylesheet.GetHashCode()}\"";
        if (Request.Headers.IfNoneMatch == eTag)
            return StatusCode(304); // Not Modified

        Response.Headers.CacheControl = "public, max-age=31536000"; // one year
        Response.Headers.ETag = eTag;

        return Content(stylesheet, "text/css");
    }
}