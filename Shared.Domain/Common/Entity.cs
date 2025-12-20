namespace Shared.Domain.Common
{
    /// <summary>
    /// Represents a base domain entity with a unique identifier and audit information.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Gets the unique identifier of the entity.
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Gets the UTC timestamp when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Gets the UTC timestamp when the entity was last updated, if any.
        /// </summary>
        public DateTime? UpdatedAt { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class
        /// with a new unique identifier and sets the creation timestamp to UTC now.
        /// </summary>
        protected Entity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier to assign to the entity.</param>
        protected Entity(Guid id)
        {
            Id = id;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current entity.
        /// Entities are considered equal if they are of the same type and have the same <see cref="Id"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current entity.</param>
        /// <returns><c>true</c> if the specified object is equal to the current entity; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not Entity other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            return Id == other.Id;
        }

        /// <summary>
        /// Returns a hash code for the entity based on its <see cref="Id"/>.
        /// </summary>
        /// <returns>A hash code for the current entity.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Determines whether two entities are equal.
        /// </summary>
        /// <param name="a">The first entity.</param>
        /// <param name="b">The second entity.</param>
        /// <returns><c>true</c> if the entities are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Entity? a, Entity? b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether two entities are not equal.
        /// </summary>
        /// <param name="a">The first entity.</param>
        /// <param name="b">The second entity.</param>
        /// <returns><c>true</c> if the entities are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Entity? a, Entity? b)
        {
            return !(a == b);
        }
    }
}
