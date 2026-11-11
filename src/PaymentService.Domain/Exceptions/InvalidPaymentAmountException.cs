namespace PaymentService.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando el monto de un pago no es mayor a cero.
/// </summary>
public sealed class InvalidPaymentAmountException : DomainException
{
    public InvalidPaymentAmountException(decimal amount)
        : base($"El monto del pago debe ser mayor a cero (recibido: {amount}).")
    {
    }
}
