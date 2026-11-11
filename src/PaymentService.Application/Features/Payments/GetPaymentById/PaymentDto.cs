using PaymentService.Domain.Entities;

namespace PaymentService.Application.Features.Payments.GetPaymentById;

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Status,
    string TransactionId,
    DateTime ProcessedAt);

internal static class PaymentMappings
{
    public static PaymentDto ToDto(this Payment payment) => new(
        payment.Id,
        payment.OrderId,
        payment.Amount.Amount,
        payment.Amount.Currency,
        payment.Status.ToString(),
        payment.TransactionId,
        payment.ProcessedAt);
}
