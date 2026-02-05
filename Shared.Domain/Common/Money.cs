namespace Shared.Domain.Common
{
    /// <summary>
    /// Represents a monetary value with an amount and a currency.
    /// This is a value object and is immutable.
    /// </summary>
    public class Money : ValueObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Money"/> class.
        /// Parameterless constructor required by EF Core.
        /// </summary>
        private Money()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Money"/> class
        /// with the specified amount and currency.
        /// </summary>
        /// <param name="amount">The monetary amount. Must be non-negative.</param>
        /// <param name="currency">The ISO currency code (e.g. USD, EUR).</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the amount is negative or the currency is null or empty.
        /// </exception>
        private Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be empty", nameof(currency));

            Amount = amount;
            Currency = currency.ToUpperInvariant();
        }

        /// <summary>
        /// Gets the monetary amount.
        /// </summary>
        public decimal Amount { get; } = 0;

        /// <summary>
        /// Gets the currency code in uppercase invariant format.
        /// </summary>
        public string Currency { get; } = string.Empty;

        /// <summary>
        /// Creates a new <see cref="Money"/> instance.
        /// </summary>
        /// <param name="amount">The monetary amount. Must be non-negative.</param>
        /// <param name="currency">The ISO currency code (e.g. USD, EUR).</param>
        /// <returns>A new <see cref="Money"/> instance.</returns>
        public static Money Create(decimal amount, string currency)
        {
            return new Money(amount, currency);
        }

        /// <summary>
        /// Adds another <see cref="Money"/> value to this instance.
        /// </summary>
        /// <param name="other">The money value to add.</param>
        /// <returns>A new <see cref="Money"/> instance representing the sum.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the currencies do not match.
        /// </exception>
        public Money Add(Money other)
        {
            return Currency != other.Currency
                ? throw new InvalidOperationException(
                    $"Cannot add money with different currencies: {Currency} and {other.Currency}")
                : new Money(Amount + other.Amount, Currency);
        }

        /// <summary>
        /// Subtracts another <see cref="Money"/> value from this instance.
        /// </summary>
        /// <param name="other">The money value to subtract.</param>
        /// <returns>A new <see cref="Money"/> instance representing the difference.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the currencies do not match.
        /// </exception>
        public Money Subtract(Money other)
        {
            return Currency != other.Currency
                ? throw new InvalidOperationException(
                    $"Cannot subtract money with different currencies: {Currency} and {other.Currency}")
                : new Money(Amount - other.Amount, Currency);
        }

        /// <summary>
        /// Multiplies the monetary amount by the specified multiplier.
        /// </summary>
        /// <param name="multiplier">The multiplier to apply.</param>
        /// <returns>A new <see cref="Money"/> instance with the multiplied amount.</returns>
        public Money Multiply(decimal multiplier)
        {
            return new Money(Amount * multiplier, Currency);
        }

        /// <summary>
        /// Returns the components used to determine value equality.
        /// </summary>
        /// <returns>An enumerable of equality components.</returns>
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        /// <summary>
        /// Returns a formatted string representation of the monetary value.
        /// </summary>
        /// <returns>A string in the format "Amount Currency".</returns>
        public override string ToString()
        {
            return $"{Amount:N2} {Currency}";
        }
    }
}
