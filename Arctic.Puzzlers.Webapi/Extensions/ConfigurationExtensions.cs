namespace Arctic.Puzzlers.Webapi.Extensions
{
    public static class ConfigurationExtensions 
    {
        public static string GetPostgresConnectionString(this IConfiguration configuration)
        {
            var host = configuration.GetValue<string>("DatabaseHost");
            var username = configuration.GetValue<string>("DatabaseUserName"); 
            var password = configuration.GetValue<string>("DatabaseUserPassword");
            var database = configuration.GetValue<string>("DatabaseName");
            return $"Host={host};Username={username};Password={password};Database={database}";
        }
    }
}
