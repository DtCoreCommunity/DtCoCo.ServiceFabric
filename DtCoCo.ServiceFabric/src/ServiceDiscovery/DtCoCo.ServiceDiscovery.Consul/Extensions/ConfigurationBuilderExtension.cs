using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using DtCoCo.ServiceDiscovery.Consul.Models;

namespace DtCoCo.ServiceDiscovery.Consul.Extensions
{
    public static class  ConfigurationBuilderExtension
    {
        public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder,
            IEnumerable<Uri> consulUris, List<string> consulPaths)
        {
            foreach (var consulPath in consulPaths)
            {
                configurationBuilder.Add(new ConsulConfigurationSource(consulUris, consulPath));
            }
            return configurationBuilder;
        }
    }
}
