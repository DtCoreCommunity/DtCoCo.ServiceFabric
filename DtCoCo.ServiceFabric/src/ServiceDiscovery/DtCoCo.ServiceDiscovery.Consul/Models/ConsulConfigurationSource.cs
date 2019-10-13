using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace DtCoCo.ServiceDiscovery.Consul.Models
{
    public class ConsulConfigurationSource:IConfigurationSource
    {
        public ConsulConfigurationSource(IEnumerable<Uri> consulUris, string path)
        {
            ConsulUris = consulUris;
            Path = path;
        }

        public IEnumerable<Uri> ConsulUris { get; set; }

        public string Path { get; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConsulConfigurationProvider(ConsulUris, Path);
        }
    }
}