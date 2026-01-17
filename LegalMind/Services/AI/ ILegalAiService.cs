using LegalMind.Web.Services.AI.Models;

namespace LegalMind.Web.Services.AI;

public interface ILegalAiService
{
    Task<LegalAnswer> AnswerAsync(string jurisdiction, string userQuestion, CancellationToken ct);
}
