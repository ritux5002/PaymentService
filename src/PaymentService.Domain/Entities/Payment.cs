using PaymentService.Domain.Enums;
using PaymentService.Domain.Exceptions;
using PaymentService.Domain.ValueObjects;

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
    public Money Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string TransactionId { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    // Constructor para EF Core: Amount se materializa como owned type
    private Payment(Guid id, Guid orderId, PaymentStatus status, string transactionId, DateTime processedAt)
    {
        Id = id;
        OrderId = orderId;
        Amount = null!;
        Status = status;
        TransactionId = transactionId;
        ProcessedAt = processedAt;
    }

    private Payment(Guid orderId, Money amount, PaymentStatus status, string transactionId)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Amount = amount;
        Status = status;
        TransactionId = transactionId;
        ProcessedAt = DateTime.UtcNow;
    }

    public static Payment Create(Guid orderId, Money amount)
    {
        if (orderId == Guid.Empty)
            throw new DomainRuleException("El ID de la orden no puede estar vacío.");

        var status = amount.Amount < ApprovalLimit ? PaymentStatus.Approved : PaymentStatus.Rejected;
        var transactionId = $"TX-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        return new Payment(orderId, amount, status, transactionId);
    }
}
