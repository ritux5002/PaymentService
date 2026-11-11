namespace PaymentService.WebApi.Contracts;

public record ProcessPaymentRequest(Guid OrderId, decimal Amount);
