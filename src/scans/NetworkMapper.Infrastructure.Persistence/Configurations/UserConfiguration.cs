using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetworkMapper.Domain.Entities;
using NetworkMapper.Infrastructure.Persistence.Constants;

namespace NetworkMapper.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(TableNamesConstants.Users);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired()
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => x.ClerkUserId)
            .IsUnique();

        builder.Property(x => x.ClerkUserId).IsRequired();
        builder.Property(x => x.Email).IsRequired(false);
        builder.Property(x => x.Name).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
