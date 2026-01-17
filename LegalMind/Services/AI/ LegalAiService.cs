using LegalMind.Web.Services.AI.Models;
using LegalMind.Web.Services.Retrieval;
using LegalMind.Web.Services.Safety;

namespace LegalMind.Web.Services.AI;

public class LegalAiService : ILegalAiService
{
    private readonly ILegalSourceRepository _sources;
    private readonly IAnswerPolicy _policy;

    public LegalAiService(ILegalSourceRepository sources, IAnswerPolicy policy)
    {
        _sources = sources;
        _policy = policy;
    }

    public async Task<LegalAnswer> AnswerAsync(string jurisdiction, string userQuestion, CancellationToken ct)
    {
        if (_policy.ShouldRefuse(userQuestion, out var reason))
        {
            return new LegalAnswer
            {
                Summary = "Unable to answer",
                Guidance = reason,
                Confidence = "Low",
                Risk = "Low"
            };
        }

        var topSources = await _sources.GetTopSourcesAsync(jurisdiction, take: 5, ct);

        // TODO: Replace with real RAG:
        // 1) Retrieve relevant chunks
        // 2) Call LLM with strict prompt + citations
        // 3) Post-validate citations + safety

        return new LegalAnswer
        {
            Summary = "General guidance (stub)",
            Guidance = $"You asked: \"{userQuestion}\".\n\nLegalMind will provide jurisdiction-aware guidance for {jurisdiction} with citations once the RAG pipeline is connected.",
            Confidence = "Low",
            Risk = "Moderate",
            Citations = topSources.Select(s => new CitedSource
            {
                Title = s.Title,
                Citation = s.Citation ?? "",
                Url = s.Url,
                LastUpdatedUtc = s.LastUpdatedUtc
            }).ToList(),
            NextSteps =
            {
                "Confirm the jurisdiction and key dates.",
                "Gather relevant documents and messages.",
                "If risk is high or time-sensitive, consult a licensed attorney."
            }
        };
    }
}
