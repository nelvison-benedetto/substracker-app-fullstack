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
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")  //nome ESATTAMENTE UGUALE a quello sul db!! oppure install plugin EFCore.'NamingConventions' che auto fa e.g.RefreshTokens -> refresh_tokens(x il db)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasConversion(
                e => e.Value,
                v => new Email(v))
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasColumnName("passwordhash")
            .HasConversion(
                p => p.Value,
                v => new PasswordHash(v))
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired().HasColumnName("isactive");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("createdat")
            .HasColumnType("timestamptz(3)")  //w timezone
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updatedat")
            .HasColumnType("timestamptz(3)")
            .IsRequired();

        builder.Property(x => x.LastLogin)
            .HasColumnName("lastlogin")
            .HasColumnType("timestamptz(3)");

        builder.Ignore(x => x.RefreshTokens);

        builder
            .HasMany<RefreshToken>("_refreshTokens")  //collection privata di User → aggregate root gestisce i figli
            .WithOne()  //ogni refresh token ha un solo user
            .HasForeignKey("UserId")  //la FK è la shadow property creata sopra
            .IsRequired()  //un token non può esistere senza user
            .OnDelete(DeleteBehavior.Cascade);  //se elimini user, cancelli tutti i suoi refresh token


    }
}

