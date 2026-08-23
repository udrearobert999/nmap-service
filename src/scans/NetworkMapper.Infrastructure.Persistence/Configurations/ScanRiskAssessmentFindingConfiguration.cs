using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Constants;

namespace NetworkMapper.Infrastructure.Persistence.Configurations;

internal sealed class ScanRiskAssessmentFindingConfiguration : IEntityTypeConfiguration<ScanRiskAssessmentFinding>
{
    public void Configure(EntityTypeBuilder<ScanRiskAssessmentFinding> builder)
    {
        builder.ToTable(TableNamesConstants.ScanRiskAssessmentFindings);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.Port).IsRequired();
        builder.Property(x => x.Service).IsRequired();
        builder.Property(x => x.Product).IsRequired(false);
        builder.Property(x => x.Version).IsRequired(false);
        builder.Property(x => x.Cpe).IsRequired(false);
        builder.Property(x => x.MatchedCves).IsRequired();
        builder.Property(x => x.CvssScore).IsRequired();
        builder.Property(x => x.KevFlag).IsRequired();
        builder.Property(x => x.MatchConfidence).IsRequired();

        builder.HasOne(x => x.ScanRiskAssessment)
            .WithMany(r => r.Findings)
            .HasForeignKey(x => x.ScanRiskAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
