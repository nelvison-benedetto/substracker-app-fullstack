using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Infrastructure.Persistence.Configurations;

//refreshtoken non vive senza user (è un aggregate).
public class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever(); //!!, dice a EF di non aspettarsi che il db generei l'id(xk lo genero io nel Domain)

        builder.Property(x => x.Token)
            .HasColumnName("token")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expiresat")
            .HasColumnType("timestamptz(3)")
            .IsRequired();

        builder.Property(x => x.IsRevoked)
            .HasColumnName("isrevoked")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("createdat")
            .HasColumnType("timestamptz(3)")  //w timezone!
            .IsRequired();


        //builder.Property<Guid>("UserId")
        //    .HasColumnName("userid"); //SHADOW FK (domain non la conosce), esiste solo dentro EF non dentro le classi c#!! FK verso User. la puoi usare nelle query w e.g. EF.Property<Guid>(s, "UserId")
        builder.Property<UserId>("UserId")  //SHADOW FK (Value Object)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .HasColumnName("userid")
            .IsRequired();

        builder.HasIndex("UserId");
        builder.HasIndex(x => x.Token)
            .IsUnique();  //ogni refresh token deve essere univoco nel DB

    }
}
