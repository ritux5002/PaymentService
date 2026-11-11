using MediatR;

namespace PaymentService.Application.Features.Payments.GetPaymentById;

public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentDto>;
