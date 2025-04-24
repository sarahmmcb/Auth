using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace AuthApi.Security
{
    public class LocalAuthMiddleware : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            var isLocal = context.Connection.RemoteIpAddress?.ToString() == "127.0.0.1"
                       || context.Connection.RemoteIpAddress?.ToString() == "::1";

            if (authorizeResult.Succeeded || isLocal)
            {
                await next(context);
                return;
            }

            // Fall back to the default implementation.
            await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
