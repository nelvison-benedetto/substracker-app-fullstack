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

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => new SubscriptionId(value))
            .HasColumnType("uuid")
            .ValueGeneratedNever(); //!!, dice a EF di non aspettarsi che il db generei l'id(xk lo genero io nel Domain)

        builder.Property<Guid>("UserId")  //SHADOW FK (domain non la conosce), esiste solo dentro EF non dentro le classi c#!! fk verso User
            .HasColumnName("userid")
            .IsRequired();  

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(x => x.BillingCycle)
            .HasColumnName("billingcycle")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("startdate")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnName("enddate");

        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasMaxLength(50);

        //x relazione User->Subscription ma senza navigation property(che è un problema xk non è loosing)!!
        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
