namespace AuthApi.Logging
{
    public static class AuthLogger
    {

        private static ILogger _logger;
        private static bool _isInitialized = false;

        public static void Initialize(ILogger logger)
        {
            if (_isInitialized)
            {
                return;
            }

            _logger = logger;
        }

        public static void LogInformation<T>(string message) => _logger.LogInformation($"{typeof(T).Name}: {message}");
        public static void LogDebug<T>(string message) => _logger.LogDebug($"{typeof(T).Name}: {message}");
        public static void LogWarning<T>(string message) => _logger.LogWarning($"{typeof(T).Name}: {message}");
        public static void LogError<T>(string message) => _logger.LogError($"{typeof(T).Name}: {message}");
        public static void LogCritical<T>(string message) => _logger.LogCritical($"{typeof(T).Name}: {message}");
    }
}
