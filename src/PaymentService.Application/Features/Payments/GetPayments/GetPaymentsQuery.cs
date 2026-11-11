using MediatR;
using PaymentService.Application.Features.Payments.GetPaymentById;

namespace PaymentService.Application.Features.Payments.GetPayments;

public record GetPaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>;
