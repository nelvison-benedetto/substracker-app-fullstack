using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    //ANCHE OutboxMessage HA BISOGNO DI UNA ..CONFIGURATION?? INFO

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id); //dici quale è la pk

        builder.Property(x => x.Id)
            .HasColumnName("id")  //nome ESATTAMENTE UGUALE a quello sul db!! oppure install plugin EFCore.'NamingConventions' che auto fa e.g.RefreshTokens -> refresh_tokens(x il db)
            .HasConversion(
                id => id.Value,  //quando save allora prendi il value
                value => new UserId(value))  //quando invece Load allora crea nuovo type 
            .HasColumnType("uuid")
            .ValueGeneratedNever();  //cosi EF sa di non aspettarsi che il db generi l'id(xk lo genero io nel Domain)!

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
            .HasColumnType("timestamptz(3)")  //w timezone!
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updatedat")
            .HasColumnType("timestamptz(3)")
            .IsRequired();

        builder.Property(x => x.LastLogin)
            .HasColumnName("lastlogin")
            .HasColumnType("timestamptz(3)");

        builder.Ignore(x => x.RefreshTokens);  //ignora xk EF altrimenti si confoderebbe tra RefreshTokens(property) e _refreshTokens(field str). intanto glielo dici here qua sotto cosa fare.

        builder
            .HasMany<RefreshToken>("_refreshTokens")  //lo trovi in User.cs, (stai passando una str non una property!, queindi ef fa 'ok, cerco un FIELD privato chiamato _refreshTokens' e lo usa direttamente)
            .WithOne()  //ogni refresh token ha un solo user
            .HasForeignKey("UserId")  //la FK è la shadow property
            .IsRequired()  //un token non può esistere senza user
            .OnDelete(DeleteBehavior.Cascade);  //se elimini user, cancelli tutti i suoi refresh token

    }
}

