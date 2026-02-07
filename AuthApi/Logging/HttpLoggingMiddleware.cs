using System.Text;

namespace AuthApi.Logging
{
    public class HttpLoggingOptions
    {
        public bool LogRequests { get; set; } = true;
        public bool LogResponse { get; set; } = true;
        public IEnumerable<string> ExcludePaths = new List<string>();
        public int MaxBodySize = 4096000; // Max log 4KB as default

        public bool ShouldLogPath(PathString path)
        {
            return !ExcludePaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpLoggingMiddleware> _logger;
        private HttpLoggingOptions _options = new HttpLoggingOptions();

        public HttpLoggingMiddleware(
            RequestDelegate next,
            ILogger<HttpLoggingMiddleware> logger,
            HttpLoggingOptions options)
        {
            _next = next;
            _logger = logger;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.ShouldLogPath(context.Request.Path))
            { 
                await _next(context);
                return;
            }

            var originalRequestBody = context.Request.Body;
            var originalResponseBody = context.Response.Body;

            try
            {
                if (_options.LogRequests)
                {
                    string requestBody = await ReadRequestBodyAsync(context);
                    LogRequest(context, requestBody);
                }

                using var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;

                await _next(context);

                if (_options.LogResponse)
                {
                    string responseBody = await ReadResponseBodyAsync(context.Response);
                    LogResponse(context, responseBody);
                }

                await responseBodyStream.CopyToAsync(originalResponseBody);
            }
            finally
            {
                context.Request.Body = originalRequestBody;
                context.Response.Body = originalResponseBody;
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpContext context)
        {
            // This makes the request body readable mutiple times
            context.Request.EnableBuffering();

            using var streamReader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            var requestBody = await streamReader.ReadToEndAsync();

            if (Encoding.UTF8.GetBytes(requestBody).Length > _options.MaxBodySize)
            {
                requestBody = TruncateMessage(requestBody);
            }

            // Reset the position so it can be read again by other logic
            context.Request.Body.Position = 0;

            return requestBody;
        }

        private async Task<string> ReadResponseBodyAsync(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            if (Encoding.UTF8.GetBytes(responseBody).Length > _options.MaxBodySize)
            {
                responseBody = TruncateMessage(responseBody);
            }

            return responseBody;
        }

        private string TruncateMessage(string message)
        {
            message = message[..((int)_options.MaxBodySize / 2)]; // hack bc most characters logged here should be 1-2 bytes in length
            return message += "... [truncated for size]";
        }

        private void LogRequest(HttpContext context, string requestBody)
        {
            _logger.LogInformation(
                $"HTTP {context.Request.Method} {context.Request.Path} " +
                $"received at {DateTime.UtcNow}\n" +
                $"Request Body: {requestBody}");
        }

        private void LogResponse(HttpContext context, string responseBody)
        {
            _logger.LogInformation(
                $"HTTP {context.Response.StatusCode} returned for {context.Request.Method}" +
                $" Path: {context.Request.Path}\n" +
                $"Response Body: {responseBody}");
        }
    }
}
