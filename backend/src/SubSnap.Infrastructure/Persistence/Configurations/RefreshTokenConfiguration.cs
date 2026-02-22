using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubSnap.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            .HasColumnType("uuid");

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

        //FK verso User
        builder.Property<Guid>("UserId")
            .HasColumnName("userid"); //SHADOW FK (domain non la conosce), esiste solo dentro EF non dentro le classi c#!!
        builder.HasIndex("UserId");
        builder.HasIndex(x => x.Token)
            .IsUnique();  //ogni refresh token deve essere univoco nel DB
    }
}
