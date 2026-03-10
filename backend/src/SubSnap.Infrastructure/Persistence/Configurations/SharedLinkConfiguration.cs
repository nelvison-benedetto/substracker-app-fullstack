using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Infrastructure.Persistence.Configurations;

public class SharedLinkConfiguration : IEntityTypeConfiguration<SharedLink>
{
    public void Configure(EntityTypeBuilder<SharedLink> builder)
    {
        builder.ToTable("sharedlinks");

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,            
                guid => new SharedLinkId(guid))
            .ValueGeneratedOnAdd();

        builder.Property(sl => sl.Link)
            .HasColumnName("link")
            .HasMaxLength(255)
            .IsRequired();  //!!!

        builder.Property(sl => sl.ExpireAt)
            .HasColumnName("expiresat");

        builder.Property(sl => sl.Views)
            .HasColumnName("views")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sl => sl.CreatedAt)
            .HasColumnName("createdat")
            .HasColumnType("timestamptz(3)")  //w timezone!
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(sl => sl.UpdatedAt)
            .HasColumnName("updatedat")
            .HasColumnType("timestamptz(3)")  //w timezone!
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property<UserId>("UserId")  //SHADOW FK (Value Object)
            .HasColumnName("userid")
            .HasConversion(
                id => id.Value,
                value => new UserId(value)
            )
            .IsRequired();

        builder.HasOne<User>()           //NON nav prop diretta, solo FK shadow
            .WithMany()
            .HasForeignKey("UserId")
            .HasConstraintName("fk_sharedlinks_users")
            .OnDelete(DeleteBehavior.Cascade);

        //Index su UserId
        builder.HasIndex("UserId")
            .HasDatabaseName("ix_sharedlinks_userid");
    }
}