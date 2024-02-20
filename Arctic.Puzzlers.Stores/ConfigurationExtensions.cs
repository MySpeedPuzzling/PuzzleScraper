using Microsoft.Extensions.Configuration;

namespace Arctic.Puzzlers.Stores
{
    public static class ConfigurationExtensions
    {
        private const string FileOutputFolderKey = "ResultOutputFolder";

        public static string GetFileOutputFolder(this IConfiguration configuration)
        {
            return configuration[FileOutputFolderKey] ?? string.Empty;
        }
    }
}
