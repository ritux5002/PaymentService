using FluentValidation;
using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Application.Features.Payments.Commands.ProcessPayment;

public record ProcessPaymentCommand(Guid OrderId, decimal Amount) : IRequest<ProcessPaymentResponse>;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResponse>
{
    private readonly IPaymentRepository _paymentRepository;

    public ProcessPaymentCommandHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<ProcessPaymentResponse> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = Payment.Process(command.OrderId, command.Amount);

        await _paymentRepository.AddAsync(payment, cancellationToken);

        return new ProcessPaymentResponse(payment.Status.ToString(), payment.TransactionId);
    }
}
