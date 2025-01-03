using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SU.Replays.Database;
using SU.Replays.Helpers;

namespace SU.Replays.Controllers;

public class DownloadController : Controller
{
    private ApplicationDbContext _dbContext;
    private ReplayHelper _replayHelper;

    public DownloadController(ApplicationDbContext dbContext, ReplayHelper replayHelper)
    {
        _dbContext = dbContext;
        _replayHelper = replayHelper;
    }

    [Authorize]
    [HttpGet("download-replay")]
    public async Task<IActionResult> Download(
        [FromQuery] string file
    )
    {
        var replay = await _dbContext.Replays
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.FileLocation == file);

        if (replay == null)
            return NotFound();

        var userId = _replayHelper.GetSsId(User);
        var groups = _replayHelper.GetGroups(User);
        if (!replay.Participants.Contains(userId) &&
            ReplayHelper.AllAccessRoles.All(r => !groups.Contains(r))
            )
        {
            return Forbid();
        }

        var rootPath = Path.GetFullPath(replay.FileLocation);
        return PhysicalFile(rootPath, "application/zip", Path.GetFileName(rootPath));
    }
}