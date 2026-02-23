using AuthApi.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Exceptions
{
    public class GenericExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
               HttpContext httpContext,
               Exception exception,
               CancellationToken cancellationToken)
        {
            AuthLogger.LogError<GenericExceptionHandler>($"Unhandled exception occurred. Type: {exception.GetType().Name} Message: {exception.Message}");

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Type = "Internal Server Error",
                    Title = "Internal Server Error",
                    Detail = "An unrecoverable error occurred"
                }
            });
        }
    }
}
