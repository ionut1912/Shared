using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Api.Extensions;

public static class HttpContextExtensions
{
    public static Guid GetAccountId(this HttpContext httpContext)
    {
        var accountId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out var guid))
            return Guid.Empty;
        return guid;
    }
}
