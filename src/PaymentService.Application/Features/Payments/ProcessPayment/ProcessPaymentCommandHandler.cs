using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Common.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Application.Features.Payments.ProcessPayment;

/// <summary>
/// Procesa el pago de una orden de forma idempotente: si ya existe un pago para ese OrderId
/// (por ejemplo, un reintento después de un timeout en el e-commerce) devuelve el mismo pago
/// en lugar de cobrar dos veces.
/// </summary>
public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ProcessPaymentResponse> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var existing = await _paymentRepository.GetByOrderIdAsync(command.OrderId, cancellationToken);
        if (existing is not null)
        {
            _logger.LogInformation(
                "Pago ya procesado para la orden {OrderId}: se devuelve {TransactionId} ({Status}).",
                command.OrderId, existing.TransactionId, existing.Status);

            return new ProcessPaymentResponse(existing.Status.ToString(), existing.TransactionId);
        }

        var payment = Payment.Create(command.OrderId, new Money(command.Amount));

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProcessPaymentResponse(payment.Status.ToString(), payment.TransactionId);
    }
}
