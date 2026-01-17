using LegalMind.Web.Domain.Entities;

namespace LegalMind.Web.Services.Retrieval;

public interface ILegalSourceRepository
{
    Task<List<LegalSource>> GetTopSourcesAsync(string jurisdiction, int take, CancellationToken ct);
}
