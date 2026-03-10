using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Infrastructure.Persistence.Configurations;

internal class SubscriptionHistoryConfiguration : IEntityTypeConfiguration<SubscriptionHistory>
{
    public void Configure(EntityTypeBuilder<SubscriptionHistory> builder)
    {
        builder.ToTable("subscriptionhistories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new Core.Domain.ValueObjects.SubscriptionHistoryId(value)
            )
            .HasColumnType("uuid")
            .ValueGeneratedNever(); //!!, dice a EF di non aspettarsi che il db generei l'id(xk lo genero io nel Domain)

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.OldValue)
            .HasColumnName("oldvalue");

        builder.Property(x => x.NewValue)
            .HasColumnName("newvalue");

        //builder.Property<Guid>("SubscriptionId")  //SHADOW KEY
        //    .HasColumnName("subscriptionid")
        //    .IsRequired(); //sempre required!!
        builder.Property<SubscriptionId>("SubscriptionId")  //SHADOW FK (Value Object)
            .HasConversion(
                id => id.Value,
                value => new SubscriptionId(value))
            .HasColumnName("subscriptionid")
            .IsRequired();

        builder.HasIndex("SubscriptionId");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("createdat")
            .HasColumnType("timestamptz(3)")  //w timezone!
            .IsRequired();


        builder
            .HasOne<Subscription>()
            .WithMany()
            .HasForeignKey("SubscriptionId")
            .OnDelete(DeleteBehavior.Cascade);

    }
}
