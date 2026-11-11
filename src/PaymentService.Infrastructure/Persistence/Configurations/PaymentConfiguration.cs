using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2).IsRequired();
            money.Property(m => m.Currency).HasMaxLength(3).IsRequired();
        });
        builder.Navigation(x => x.Amount).IsRequired();

        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.TransactionId).HasMaxLength(20).IsRequired();

        // Un pago por orden. El provider InMemory no hace cumplir índices únicos:
        // la idempotencia real la garantiza ProcessPaymentCommandHandler buscando por OrderId.
        builder.HasIndex(x => x.OrderId).IsUnique();
    }
}
