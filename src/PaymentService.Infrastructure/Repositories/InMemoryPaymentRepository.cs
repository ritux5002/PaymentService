using System.Collections.Concurrent;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.Repositories;

/// <summary>
/// Repositorio en memoria: alcanza para un simulador de pagos sin estado persistente real.
/// Se registra como Singleton para que los datos sobrevivan entre requests dentro del mismo proceso.
/// </summary>
public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly ConcurrentDictionary<Guid, Payment> _payments = new();

    public Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        _payments[payment.Id] = payment;
        return Task.CompletedTask;
    }

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var payment = _payments.Values.FirstOrDefault(p => p.OrderId == orderId);
        return Task.FromResult(payment);
    }
}
