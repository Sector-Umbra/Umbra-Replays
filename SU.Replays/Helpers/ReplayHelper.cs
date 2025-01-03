using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SU.Replays.Database;
using SU.Replays.Database.Models;

namespace SU.Replays.Helpers;

public class ReplayHelper
{
    public static readonly List<string> AllAccessRoles = new()
    {
        "Game Master",
        "Project Manager",
    };

    private readonly ApplicationDbContext _context;

    public ReplayHelper(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Replay>> GetAllReplays()
    {
        return await _context.Replays
            .AsNoTracking() // performance optimization
            .ToListAsync();
    }

    public Guid GetSsId(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue("ss14-id");
        if (userId == null)
        {
            throw new ArgumentException("User does not have an SS14 ID claim.");
        }
        return Guid.Parse(userId);
    }

    /// <summary>
    /// Gets only the replays where the user ID is a participant.
    /// </summary>
    public async Task<List<Replay>> GetFilteredReplays(Guid userId)
    {
        return await _context.Replays
            .AsNoTracking()
            .Where(r => r.Participants.Any(p => p == userId))
            .ToListAsync();
    }

    public List<string> GetGroups(ClaimsPrincipal user)
    {
        var groups = user.FindAll("groups");
        return groups.Select(g => g.Value).ToList();
    }
}