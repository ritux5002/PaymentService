using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Features.Payments.Commands.ProcessPayment;

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
}

public record ProcessPaymentRequest(Guid OrderId, decimal Amount);
