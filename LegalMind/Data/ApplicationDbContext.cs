using LegalMind.Web.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LegalMind.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<LegalSource> LegalSources => Set<LegalSource>();
    public DbSet<ChatThread> ChatThreads => Set<ChatThread>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<LegalSource>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).HasMaxLength(300).IsRequired();
            b.Property(x => x.Jurisdiction).HasMaxLength(120).IsRequired();
            b.Property(x => x.SourceType).HasMaxLength(50).IsRequired();
            b.Property(x => x.Url).HasMaxLength(800);
            b.Property(x => x.LastUpdatedUtc).IsRequired();
            b.Property(x => x.Status).HasMaxLength(30).IsRequired();
            b.HasIndex(x => new { x.Jurisdiction, x.SourceType, x.Status });
        });

        builder.Entity<ChatThread>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Jurisdiction).HasMaxLength(120).IsRequired();
            b.Property(x => x.CreatedUtc).IsRequired();
            b.HasIndex(x => new { x.UserId, x.CreatedUtc });
        });

        builder.Entity<ChatMessage>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Role).HasMaxLength(20).IsRequired(); // user/assistant/system
            b.Property(x => x.Content).IsRequired();
            b.Property(x => x.CreatedUtc).IsRequired();
            b.HasOne(x => x.Thread)
                .WithMany(t => t.Messages)
                .HasForeignKey(x => x.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ThreadId, x.CreatedUtc });
        });
    }
}
