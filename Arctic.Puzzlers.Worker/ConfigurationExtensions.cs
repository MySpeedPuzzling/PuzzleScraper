using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctic.Puzzlers.Worker
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
