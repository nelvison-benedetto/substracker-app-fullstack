using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Infrastructure.Persistence.Configurations;

public sealed class UserMediaConfiguration
    : IEntityTypeConfiguration<UserMedia>
{
    public void Configure(EntityTypeBuilder<UserMedia> builder)
    {
        builder.ToTable("usermedia");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new UserMediaId(value))
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        //builder.Property(x => x.UserId)
        //    .HasColumnName("userid")
        //    .HasConversion(
        //        id => id.Value,
        //        v => new UserId(v))
        //    .HasColumnType("uuid")
        //    .IsRequired();

        builder.Property<Guid>("UserId")  //SHADOW FK
            .HasColumnName("userid")
            .IsRequired();

        builder.Property(x => x.ObjectKey)
            .HasColumnName("objectkey")
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasColumnName("contenttype")
            .HasMaxLength(100);

        builder.Property(x => x.Size)
            .HasColumnName("size");

        builder.Property(x => x.UploatedAt)
            .HasColumnName("uploadedat")
            .HasColumnType("timestamptz(3)")
            .IsRequired();
    }
}