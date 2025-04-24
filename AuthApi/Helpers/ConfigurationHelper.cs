namespace AuthApi.Helpers
{
    public static class ConfigurationHelper
    {
        private static IConfiguration _config;

        public static void Initialize(IConfiguration config)
        {
            _config = config;
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
