using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.WebApi.Contracts;
using PaymentService.Application.Features.Payments.ProcessPayment;
using PaymentService.Application.Features.Payments.GetPaymentById;
using PaymentService.Application.Features.Payments.GetPayments;

namespace PaymentService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Procesa un intento de pago para una orden. No se integra ninguna pasarela real:
    /// se aprueba si el monto es menor a 100000, se rechaza en caso contrario
    /// (ver PaymentService.Domain.Entities.Payment.ApprovalLimit).
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> Process(
        [FromBody] ProcessPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ProcessPaymentCommand(request.OrderId, request.Amount);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos los pagos procesados.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un pago por su Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(id), cancellationToken);
        return Ok(result);
    }
}
