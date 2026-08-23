using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Constants;

namespace NetworkMapper.Infrastructure.Persistence.Configurations;

internal sealed class ScanConfiguration : IEntityTypeConfiguration<Scan>
{
    public void Configure(EntityTypeBuilder<Scan> builder)
    {
        builder.ToTable(TableNamesConstants.Scans);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired()
            .ValueGeneratedOnAdd();

        builder.HasIndex(s => s.RequestId)
            .IsUnique();

        builder.HasIndex(s => s.TeamId);

        builder.Property(x => x.Target).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CompletedAt).IsRequired(false);
        builder.Property(x => x.ErrorMessage).IsRequired(false);

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