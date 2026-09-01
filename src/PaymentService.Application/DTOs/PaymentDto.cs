namespace PaymentService.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Status,
    string TransactionId,
    DateTime ProcessedAt);
