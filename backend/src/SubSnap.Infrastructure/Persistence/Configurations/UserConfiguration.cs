using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(x => x.Email)
            .HasConversion(
                e => e.Value,
                v => new Email(v))
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasConversion(
                p => p.Value,
                v => new PasswordHash(v))
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp(3) without time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp(3) without time zone")
            .IsRequired();

        builder.Property(x => x.LastLogin)
            .HasColumnType("timestamp(3) without time zone");

        builder.Ignore(x => x.RefreshTokens);
    }
}

