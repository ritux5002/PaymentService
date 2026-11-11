using MediatR;

namespace PaymentService.Application.Features.Payments.ProcessPayment;

public record ProcessPaymentCommand(Guid OrderId, decimal Amount) : IRequest<ProcessPaymentResponse>;
