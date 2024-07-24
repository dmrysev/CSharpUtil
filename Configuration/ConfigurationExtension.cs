using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CSharpUtil.Configuration
{
    public static class ConfigurationRootExtensions
    {
        public static bool HasValue(this IConfigurationRoot config, string key)
        {
            var value = config[key];
            return value != null && value != string.Empty;
        }

        public static string GetNullableValue(this IConfigurationRoot config, string key)
        {
            if (config.HasValue(key))
            {
                return config[key];
            }
            return null;
        }

        public static IEnumerable<string> GetArray(this IConfigurationRoot config, string key)
        {
            return config.GetSection(key).Get<IEnumerable<string>>();
        }
    }
}