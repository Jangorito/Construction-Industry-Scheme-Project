using CisCompliance.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CisCompliance.Infrastructure.Persistence;

public sealed class CisComplianceDbContext : DbContext
{
    public CisComplianceDbContext(DbContextOptions<CisComplianceDbContext> options) : base(options)
    {
    }

    public DbSet<InvoiceEntity> Invoices => Set<InvoiceEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InvoiceEntity>(e =>
        {
            e.HasKey(i => i.InvoiceId);
            e.Property(i => i.InvoiceId).HasMaxLength(64);
            e.HasMany(i => i.LineItems).WithOne(l => l.Invoice)
                .HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceLineEntity>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasIndex(l => new { l.InvoiceId, l.LineId }).IsUnique();

            // Explicit precision: without it, EF Core warns that decimal
            // values could be silently truncated. 18,2 comfortably covers
            // any realistic invoice line amount in pounds and pence.
            e.Property(l => l.AmountGbp).HasPrecision(18, 2);

            // Stored as readable text (e.g. "Construction"), not a raw int,
            // this is a compliance tool a reviewer may plausibly open
            // directly in SSMS. Independent of the JsonStringEnumConverter
            // on these same enums, that one only governs the HTTP JSON
            // boundary, this one only governs the SQL column, no conflict.
            e.Property(l => l.PredictedLabel).HasConversion<string>().HasMaxLength(20);
            e.Property(l => l.PredictedConfidence).HasConversion<string>().HasMaxLength(10);
            e.Property(l => l.EffectiveLabel).HasConversion<string>().HasMaxLength(20);

            e.OwnsMany(l => l.Citations, c =>
            {
                c.WithOwner().HasForeignKey("InvoiceLineEntityId");
                c.Property(x => x.RuleId).HasMaxLength(32);
            });
        });
    }
}
