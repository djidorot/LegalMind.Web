using LegalMind.Web.Data;
using LegalMind.Web.Domain.Entities;
using LegalMind.Web.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegalMind.Web.Controllers;

[Authorize]
public class AskController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILegalAiService _ai;
    private readonly UserManager<IdentityUser> _users;

    public AskController(ApplicationDbContext db, ILegalAiService ai, UserManager<IdentityUser> users)
    {
        _db = db;
        _ai = ai;
        _users = users;
    }

    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask(string jurisdiction, string question, CancellationToken ct)
    {
        jurisdiction = string.IsNullOrWhiteSpace(jurisdiction) ? "PH" : jurisdiction.Trim();

        var user = await _users.GetUserAsync(User);
        if (user is null) return Challenge();

        var thread = new ChatThread
        {
            UserId = user.Id,
            Jurisdiction = jurisdiction,
            Title = question.Length > 60 ? question[..60] + "..." : question
        };

        thread.Messages.Add(new ChatMessage { Role = "user", Content = question });

        var answer = await _ai.AnswerAsync(jurisdiction, question, ct);

        thread.Messages.Add(new ChatMessage
        {
            Role = "assistant",
            Content =
                $"{answer.Summary}\n\n{answer.Guidance}\n\nConfidence: {answer.Confidence}\nRisk: {answer.Risk}\n\n{answer.Disclaimer}"
        });

        _db.ChatThreads.Add(thread);
        await _db.SaveChangesAsync(ct);

        return RedirectToAction(nameof(Thread), new { id = thread.Id });
    }

    public async Task<IActionResult> Thread(long id, CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return Challenge();

        var thread = await _db.ChatThreads
            .Include(t => t.Messages.OrderBy(m => m.CreatedUtc))
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id, ct);

        if (thread is null) return NotFound();

        return View(thread);
    }
}
