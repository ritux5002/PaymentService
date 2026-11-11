using MediatR;
using PaymentService.Application.Features.Payments.GetPaymentById;
using PaymentService.Application.Common.Interfaces;

namespace PaymentService.Application.Features.Payments.GetPayments;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentsQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<IReadOnlyList<PaymentDto>> Handle(GetPaymentsQuery query, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetAllAsync(cancellationToken);

        return payments
            .Select(payment => payment.ToDto())
            .ToList();
    }
}
