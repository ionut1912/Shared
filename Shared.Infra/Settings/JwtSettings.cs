namespace Shared.Infra.Settings
{
    /// <summary>
    /// Represents configuration settings for JWT (JSON Web Token) authentication.
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Gets or sets the secret key used to sign the JWT tokens.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the issuer of the JWT tokens.
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the audience for the JWT tokens.
        /// </summary>
        public string Audience { get; set; } = string.Empty;
    }
}
