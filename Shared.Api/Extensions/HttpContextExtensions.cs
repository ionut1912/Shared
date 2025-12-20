using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Shared.Api.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="HttpContext"/>.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Retrieves the account identifier (as a <see cref="Guid"/>) from the current <see cref="HttpContext"/> user claims.
        /// Returns <see cref="Guid.Empty"/> if the claim is missing or invalid.
        /// </summary>
        /// <param name="httpContext">The HTTP context containing the user claims.</param>
        /// <returns>The account identifier as a <see cref="Guid"/> or <see cref="Guid.Empty"/>.</returns>
        public static Guid GetAccountId(this HttpContext httpContext)
        {
            var accountId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out var guid))
                return Guid.Empty;
            return guid;
        }
    }
}
