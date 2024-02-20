using Microsoft.Extensions.Configuration;

namespace Arctic.Puzzlers.Stores
{
    public static class ConfigurationExtensions
    {
        private const string FileOutputFolderKey = "ResultOutputFolder";
        private const string OutputTypesKey = "OutputTypes";

        public static string GetFileOutputFolder(this IConfiguration configuration)
        {
            return configuration[FileOutputFolderKey] ?? string.Empty;
        }

        public static string GetOutputTypes(this IConfiguration configuration)
        {
            return configuration[OutputTypesKey] ?? string.Empty;
        }
    }
}
