using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DtCoCo.ServiceDiscovery.Consul.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DtCoCo.Ocelot.Extension
{
    public static class HostingEnvironmentExtensions
    {
        private static readonly ConcurrentDictionary<string, IConfigurationRoot> _configurationCache;

        static HostingEnvironmentExtensions()
        {
            _configurationCache = new ConcurrentDictionary<string, IConfigurationRoot>();
        }
          
        public static IConfigurationRoot GetAppConfiguration(this IHostingEnvironment env)
        {
            var cacheKey = env.ContentRootPath + "#" + env.EnvironmentName + "#" + env.IsDevelopment();
            return _configurationCache.GetOrAdd(cacheKey, _ => )
        }

        private static IConfigurationRoot BuildConfiguration(string path, string environmentName = null,
            bool addUserSecrets = false)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile("appsettings.json", true, true);
            if (!string.IsNullOrEmpty(environmentName))
            {
                builder = builder.AddJsonFile($"appsettings.{environmentName}.json", true);
            }

            builder = builder.AddEnvironmentVariables();
            var configuration = builder.Build();
            var url = configuration.GetValue<Uri>("consul:url");
            if (url != null)
            {
                builder.AddConsul(new[] {configuration.GetValue<Uri>("consul:url")},
                    configuration.GetSection("consul:path").Get<List<string>>());
            }

            return builder.Build();
        }

    }
}