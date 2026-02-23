namespace AuthApi.Helpers
{
    public static class ConfigurationHelper
    {
        private static IConfiguration _config;
        private static bool _isInitialized = false;

        public static void Initialize(IConfiguration config)
        {
            if (_isInitialized == true)
            {
                throw new InvalidOperationException("ConfigurationHelper already initialized");
            } 

            _config = config;
            _isInitialized = true;
        }

        public static IConfigurationSection Section(string key)
        {
            return _config.GetSection(key);
        }

        public static string Setting(string key)
        {
            return _config.GetSection(key).Value ?? "";
        }
    }
}
