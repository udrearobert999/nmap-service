using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vantage.Domain.Entities;
using Vantage.Infrastructure.Persistence.Constants;

namespace Vantage.Infrastructure.Persistence.Configurations;

internal sealed class ScanRiskAssessmentConfiguration : IEntityTypeConfiguration<ScanRiskAssessment>
{
    public void Configure(EntityTypeBuilder<ScanRiskAssessment> builder)
    {
        builder.ToTable(TableNamesConstants.ScanRiskAssessments);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired()
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => x.RequestId)
            .IsUnique();

        builder.HasIndex(x => x.TeamId);

        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.RequestedAt).IsRequired();
        builder.Property(x => x.CompletedAt).IsRequired(false);
        builder.Property(x => x.ErrorMessage).IsRequired(false);
        builder.Property(x => x.OverallRiskScore).IsRequired(false);

        builder.HasOne(x => x.Scan)
            .WithMany()
            .HasForeignKey(x => x.ScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
