namespace Shared.Domain.Common
{
    /// <summary>
    /// Represents a base class for value objects in the domain.
    /// Value objects are compared based on their properties rather than identity.
    /// </summary>
    public abstract class ValueObject
    {
        /// <summary>
        /// Returns an ordered collection of the components that define equality for this value object.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{Object}"/> of components used for equality comparison.</returns>
        protected abstract IEnumerable<object?> GetEqualityComponents();

        /// <summary>
        /// Determines whether the specified object is equal to the current value object.
        /// Equality is based on the values of the components returned by <see cref="GetEqualityComponents"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current value object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current value object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <summary>
        /// Returns a hash code for this value object based on its equality components.
        /// </summary>
        /// <returns>A hash code for the current value object.</returns>
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x?.GetHashCode() ?? 0)
                .Aggregate((x, y) => x ^ y);
        }

        /// <summary>
        /// Determines whether two value objects are equal.
        /// </summary>
        /// <param name="left">The first value object.</param>
        /// <param name="right">The second value object.</param>
        /// <returns><c>true</c> if the value objects are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two value objects are not equal.
        /// </summary>
        /// <param name="left">The first value object.</param>
        /// <param name="right">The second value object.</param>
        /// <returns><c>true</c> if the value objects are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(ValueObject? left, ValueObject? right)
        {
            return !(left == right);
        }
    }
}
