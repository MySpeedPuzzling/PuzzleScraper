namespace Arctic.Puzzlers.Webapi
{
    public static class ConfigurationExtensions
    {
        public static string RunModeKey = "RUNMODE";
        private const string ParseTypesKey = "PARSETYPES";
        public static string GetRunMode(this IConfiguration configuration)
        {
            return configuration[RunModeKey] ?? "48:00:00";
        }
        public static string[] GetParseTypes(this IConfiguration configuration)
        {
            return configuration[ParseTypesKey]?.Split(',') ?? new string[0];
        }
    }
}
