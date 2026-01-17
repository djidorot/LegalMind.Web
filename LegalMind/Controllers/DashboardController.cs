using System.Security.Claims;
using LegalMind.Web.Data;
using LegalMind.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegalMind.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            // Shouldn't happen when [Authorize] is working, but keeps the page safe.
            return Challenge();
        }

        var recentThreads = await _db.ChatThreads
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedUtc)
            .Select(t => new DashboardThreadItem
            {
                ThreadId = t.Id,
                Title = t.Title,
                Jurisdiction = t.Jurisdiction,
                CreatedUtc = t.CreatedUtc,
                MessageCount = t.Messages.Count
            })
            .Take(12)
            .ToListAsync(cancellationToken);

        var vm = new DashboardViewModel
        {
            DisplayName = User.Identity?.Name,
            RecentThreads = recentThreads
        };

        return View(vm);
    }
}
