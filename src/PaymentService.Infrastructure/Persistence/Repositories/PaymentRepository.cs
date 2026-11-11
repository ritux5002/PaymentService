using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Common.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
    }

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .OrderBy(p => p.ProcessedAt)
            .ToListAsync(cancellationToken);
    }
}
