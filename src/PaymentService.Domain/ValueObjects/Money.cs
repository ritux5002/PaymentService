using PaymentService.Domain.Exceptions;

namespace PaymentService.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el monto de un pago (monto + moneda).
/// Inmutable y con igualdad por valor. Un pago siempre tiene monto positivo.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public const string DefaultCurrency = "ARS";

    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = DefaultCurrency)
    {
        if (amount <= 0)
            throw new InvalidPaymentAmountException(amount);

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainRuleException("La moneda no puede estar vacía.");

        Amount = amount;
        Currency = currency;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public bool Equals(Money? other) => other is not null && Amount == other.Amount && Currency == other.Currency;

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => $"{Amount:F2} {Currency}";
}
