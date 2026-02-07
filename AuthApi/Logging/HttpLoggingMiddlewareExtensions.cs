
namespace AuthApi.Logging
{
    public static class HttpLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomHttpLogging(
            this IApplicationBuilder app,
            Action<HttpLoggingOptions> configureOptions = null)
        {
           var options = new HttpLoggingOptions();
            configureOptions?.Invoke(options);

           return app.UseMiddleware<HttpLoggingMiddleware>(options);
        }

    }
}
