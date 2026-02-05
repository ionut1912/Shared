namespace Shared.Application.Mediator
{
    /// <summary>
    /// Represents a void-like type, used when a method or request does not return a value.
    /// </summary>
    /// <remarks>
    /// Since C# does not allow <c>void</c> as a generic type parameter, <see cref="Unit"/> can be used
    /// to represent an empty return type in generic contexts, such as MediatR requests.
    /// </remarks>
    public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
    {
        /// <summary>
        /// Singleton instance of <see cref="Unit"/>.
        /// </summary>
        public static readonly Unit Value = new();

        /// <summary>
        /// Compares this instance to another <see cref="Unit"/> instance.
        /// Always returns 0 since all <see cref="Unit"/> instances are equal.
        /// </summary>
        /// <param name="other">The other <see cref="Unit"/> instance to compare.</param>
        /// <returns>0, indicating equality.</returns>
        public int CompareTo(Unit other) => 0;

        /// <summary>
        /// Determines whether this instance is equal to another <see cref="Unit"/> instance.
        /// </summary>
        /// <param name="other">The other <see cref="Unit"/> instance.</param>
        /// <returns><c>true</c> since all <see cref="Unit"/> instances are considered equal.</returns>
        public bool Equals(Unit other) => true;

        /// <summary>
        /// Determines whether this instance is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="Unit"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is Unit;

        /// <summary>
        /// Returns a hash code for this instance.
        /// Always returns 0 since all <see cref="Unit"/> instances are equal.
        /// </summary>
        /// <returns>0.</returns>
        public override int GetHashCode() => 0;

        /// <summary>
        /// Determines whether two <see cref="Unit"/> instances are equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><c>true</c> always.</returns>
        public static bool operator ==(Unit left, Unit right) => true;

        /// <summary>
        /// Determines whether two <see cref="Unit"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><c>false</c> always.</returns>
        public static bool operator !=(Unit left, Unit right) => false;

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>A string representing an empty tuple: "()".</returns>
        public override string ToString() => "()";
    }
}
