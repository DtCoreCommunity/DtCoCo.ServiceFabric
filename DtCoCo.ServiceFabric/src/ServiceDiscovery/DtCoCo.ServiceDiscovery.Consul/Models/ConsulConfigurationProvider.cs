using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DtCoCo.ServiceFabric.Utility;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace DtCoCo.ServiceDiscovery.Consul.Models
{
    public class ConsulConfigurationProvider:ConfigurationProvider
    {
        private const string ConsulIndexHeader = "X-Consul-Index";
        private readonly string _path;
        private readonly HttpClient _httpClient;
        private readonly IReadOnlyList<Uri> _consulUris;
        private readonly Task _configurationListeningTask;
        private int _failureCount;
        private int _consulUriIndex;
        private int _consulConfigurationIndex;

        public ConsulConfigurationProvider(IEnumerable<Uri> consulUris, string path)
        {
            _path = path;
            _consulUris = consulUris.Select(e => new Uri(e, $"v1/kv/{path}")).ToList();
            if (_consulUris.Count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(consulUris));
            }

            _httpClient = new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            },true);
            _configurationListeningTask = new Task(ListenToConfigurationChanges);

        }

        public override void Load() => LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        private async Task LoadAsync()
        {
            Data = await ExcuteQueryAsync();
            if (_configurationListeningTask.Status == TaskStatus.Created)
                _configurationListeningTask.Start();
        }

        private async void ListenToConfigurationChanges()
        {
            while (true)
            {
                try
                {
                    if (_failureCount > _consulUris.Count)
                    {
                        _failureCount = 0;
                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }

                    Data = await ExcuteQueryAsync(true);
                    OnReload();
                    _failureCount = 0;
                }
                catch (TaskCanceledException e)
                {
                    _failureCount = 0;
                }
                catch
                {
                    _consulUriIndex = (_consulUriIndex + 1) % _consulUris.Count;
                    _failureCount++;
                }
            }
        }

        private async Task<IDictionary<string, string>> ExcuteQueryAsync(bool isBlocking = false)
        {
            var requestUri = isBlocking ? $"?recurse=true&index={_consulConfigurationIndex}" : $"?recurse=true";
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_consulUris[_consulUriIndex],requestUri)))
            {
                using (var response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.Headers.Contains(ConsulIndexHeader))
                    {
                        var indexValue = response.Headers.GetValues(ConsulIndexHeader).FirstOrDefault();
                        int.TryParse(indexValue, out _consulConfigurationIndex);
                    }

                    var configValues =
                        JsonHelper.FromJsonList<ConsulConfigurationEntity>(await response.Content.ReadAsStringAsync());
                    var keyValues = configValues.Select(e =>
                            new KeyValuePair<string, JToken>(e.Key.Substring(_path.Length),
                                e.Value != null
                                    ? JToken.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(e.Value)))
                                    : null))
                        .Where(v => !string.IsNullOrWhiteSpace(v.Key))
                        .SelectMany(Flatten)
                        .ToDictionary(k => ConfigurationPath.Combine(k.Value.Split('/')), v => v.Value,
                            StringComparer.OrdinalIgnoreCase);
                    return keyValues;
                }
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> Flatten(KeyValuePair<string, JToken> tuple)
        {
            if (!(tuple.Value is JObject value))
            {
                if (tuple.Value is JArray values)
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        foreach (var item in Flatten(new KeyValuePair<string, JToken>($"{tuple.Key}:{i.ToString()}", values[i])))
                            yield
                                return item;
                    }
                }
                else
                {
                    var propertyKey = $"{tuple.Key}";
                    var str = tuple.Value.Value<string>();
                    yield
                        return new KeyValuePair<string, string>(propertyKey, str);
                }
            }
            else
            {
                foreach (var property in value)
                {
                    var propertyKey = $"{tuple.Key}/{property.Key}";
                    switch (property.Value.Type)
                    {
                        case JTokenType.Object:
                            foreach (var item in Flatten(new KeyValuePair<string, JToken>(propertyKey, property.Value)))
                                yield
                                    return item;
                            break;
                        case JTokenType.Array:
                            if (property.Value is JArray values)
                            {
                                for (int i = 0; i < values.Count; i++)
                                {
                                    foreach (var item in Flatten(new KeyValuePair<string, JToken>($"{propertyKey}:{i.ToString()}", values[i])))
                                        yield
                                            return item;
                                }
                            }
                            break;
                        default:
                            yield
                                return new KeyValuePair<string, string>(propertyKey, property.Value.Value<string>());
                            break;
                    }
                }
            }
        }
    }
}