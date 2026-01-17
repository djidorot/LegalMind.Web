using LegalMind.Web.Data;
using LegalMind.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LegalMind.Web.Services.Retrieval;

public class LegalSourceRepository : ILegalSourceRepository
{
    private readonly ApplicationDbContext _db;
    public LegalSourceRepository(ApplicationDbContext db) => _db = db;

    public Task<List<LegalSource>> GetTopSourcesAsync(string jurisdiction, int take, CancellationToken ct)
    {
        return _db.LegalSources
            .AsNoTracking()
            .Where(x => x.Jurisdiction == jurisdiction && x.Status == "Verified")
            .OrderByDescending(x => x.LastUpdatedUtc)
            .Take(take)
            .ToListAsync(ct);
    }
}
