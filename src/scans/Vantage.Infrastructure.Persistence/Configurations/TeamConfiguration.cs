using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vantage.Domain.Entities;
using Vantage.Infrastructure.Persistence.Constants;

namespace Vantage.Infrastructure.Persistence.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable(TableNamesConstants.Teams);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired()
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => x.ClerkOrgId)
            .IsUnique();

        builder.Property(x => x.ClerkOrgId).IsRequired();
        builder.Property(x => x.Name).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
