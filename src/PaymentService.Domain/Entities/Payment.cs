using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Registro del resultado de un intento de pago procesado para una orden del e-commerce.
/// </summary>
public class Payment
{
    /// <summary>
    /// Regla de aprobación explícita del simulador: se aprueba todo pago cuyo monto
    /// sea estrictamente menor a este límite. Montos iguales o mayores se rechazan.
    /// No se integra ninguna pasarela real; la decisión es puramente local y determinística.
    /// </summary>
    public const decimal ApprovalLimit = 100_000m;

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string TransactionId { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    private Payment(Guid orderId, decimal amount, PaymentStatus status, string transactionId)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Amount = amount;
        Status = status;
        TransactionId = transactionId;
        ProcessedAt = DateTime.UtcNow;
    }

    public static Payment Process(Guid orderId, decimal amount)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("El ID de la orden no puede estar vacío.", nameof(orderId));

        if (amount <= 0)
            throw new ArgumentException("El monto debe ser mayor a cero.", nameof(amount));

        var status = amount < ApprovalLimit ? PaymentStatus.Approved : PaymentStatus.Rejected;
        var transactionId = $"TX-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        return new Payment(orderId, amount, status, transactionId);
    }
}
