namespace Arctic.Puzzlers.Webapi
{
    public static class ConfigurationExtensions
    {
        public static string RunModeKey = "RunMode";
        public static string GetRunMode(this IConfiguration configuration)
        {
            return configuration[RunModeKey] ?? string.Empty;
        }
    }
}
