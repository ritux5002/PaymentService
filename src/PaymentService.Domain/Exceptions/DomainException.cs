namespace PaymentService.Domain.Exceptions;

/// <summary>
/// Clase base abstracta para todas las excepciones de dominio (se responden con 422).
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
