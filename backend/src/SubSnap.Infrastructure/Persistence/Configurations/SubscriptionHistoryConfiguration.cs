using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubSnap.Core.Domain.Entities;

namespace SubSnap.Infrastructure.Persistence.Configurations;

internal class SubscriptionHistoryConfiguration : IEntityTypeConfiguration<SubscriptionHistory>
{
    public void Configure(EntityTypeBuilder<SubscriptionHistory> builder)
    {
        builder.ToTable("subscriptionhistories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.OldValue)
            .HasColumnName("oldvalue");

        builder.Property(x => x.NewValue)
            .HasColumnName("newvalue");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("createdat")
            .HasColumnType("timestamptz(3)");

        builder.Property<Guid>("SubscriptionId")  //SHADOW KEY
            .HasColumnName("subscriptionid");

        builder.HasIndex("SubscriptionId");

        builder
            .HasOne<Subscription>()
            .WithMany()
            .HasForeignKey("SubscriptionId")
            .OnDelete(DeleteBehavior.Cascade);

    }
}
