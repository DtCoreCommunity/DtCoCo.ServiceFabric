using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using DtCoCo.Ocelot.Extension;
using DtCoCo.Ocelot.Extension.Configurations;
using DtCoCo.Ocelot.Persistence.Mysql.Entity;
using DtCoCo.ServiceFabric.Utility;
using Microsoft.AspNetCore.Hosting;
using MySql.Data.MySqlClient;
using Ocelot.Cache;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Responses;

namespace DtCoCo.Ocelot.Persistence.Mysql
{
    public class MySqlFileConfigurationRepository:IFileConfigurationRepository
    {
        private readonly IOcelotCache<FileConfiguration> _cache;
        private readonly ConfigAuthLimitCacheOptions _option;
        private readonly IHostingEnvironment _hostingEnvironment;

        public MySqlFileConfigurationRepository(IOcelotCache<FileConfiguration> cache, ConfigAuthLimitCacheOptions option, IHostingEnvironment hostingEnvironment)
        {
            _cache = cache;
            _option = option;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// 从数据库中提取配置信息
        /// </summary>
        /// <returns></returns>
        public async Task<Response<FileConfiguration>> Get()
        {
            var config = _cache.Get(_option.CachePrefix + "FileConfiguration", "");
            if (config != null)
            {
                return new OkResponse<FileConfiguration>(config);
            }

            #region 提取配置信息

            var configuration = _hostingEnvironment.GetAppConfiguration();
            var gatewayName = configuration["ServiceDiscovery:ServiceName"];
            var file = new FileConfiguration();
            using (var connection = new MySqlConnection(_option.DbConnectionStrings))
            {
                string glbSql =
                    "SELECT * FROM t_ocelot_global_configuration WHERE GatewayName=@GatewayName and IsDeleted=0";

                //提取全局配置信息
                var result =
                    await connection.QueryFirstOrDefaultAsync<OcelotGlobalConfiguration>(glbSql,
                        new {GatewayName = gatewayName});

                if (result != null)
                {
                    var glb = new FileGlobalConfiguration()
                    {
                        BaseUrl = result.BaseUrl,
                        DownstreamScheme = result.DownstreamScheme,
                        RequestIdKey = result.RequestIdKey
                    };
                    if (!string.IsNullOrEmpty(result.HttpHandlerOptions))
                    {
                        glb.HttpHandlerOptions = result.LoadBalancerOptions.ToObject<FileHttpHandlerOptions>();
                    }

                    if (!string.IsNullOrEmpty(result.LoadBalancerOptions))
                    {
                        glb.LoadBalancerOptions = result.LoadBalancerOptions.ToObject<FileLoadBalancerOptions>();
                    }

                    if (!string.IsNullOrEmpty(result.QoSOptions))
                    {
                        glb.QoSOptions = result.QoSOptions.ToObject<FileQoSOptions>();
                    }

                    if (!string.IsNullOrEmpty(result.ServiceDiscoveryProvider))
                    {
                        glb.ServiceDiscoveryProvider =
                            result.ServiceDiscoveryProvider.ToObject<FileServiceDiscoveryProvider>();
                    }

                    file.GlobalConfiguration = glb;

                    string routeSql =
                        "select * from t_ocelot_re_routes where OcelotGlobalConfigurationId=@OcelotGlobalConfigurationId and IsDeleted=0";
                    var routeresult =
                        (await connection.QueryAsync<OcelotReRoutes>(routeSql,
                            new {OcelotGlobalConfigurationId = result.Id})).AsList();
                    if (routeresult != null && routeresult.Count > 0)
                    {
                        var reroutelist = new List<FileReRoute>();
                        foreach (var model in routeresult)
                        {
                            var m = new FileReRoute();
                            if (!string.IsNullOrEmpty(model.AuthenticationOptions))
                            {
                                m.AuthenticationOptions =
                                    model.AuthenticationOptions.ToObject<FileAuthenticationOptions>();
                            }

                            if (!string.IsNullOrEmpty(model.CacheOptions))
                            {
                                m.FileCacheOptions = model.CacheOptions.ToObject<FileCacheOptions>();
                            }

                            if (!string.IsNullOrEmpty(model.DelegatingHandlers))
                            {
                                m.DelegatingHandlers = model.DelegatingHandlers.ToObject<List<string>>();
                            }

                            if (!string.IsNullOrEmpty(model.LoadBalancerOptions))
                            {
                                m.LoadBalancerOptions = model.LoadBalancerOptions.ToObject<FileLoadBalancerOptions>();
                            }

                            if (!string.IsNullOrEmpty(model.QoSOptions))
                            {
                                m.QoSOptions = model.QoSOptions.ToObject<FileQoSOptions>();
                            }

                            if (!string.IsNullOrEmpty(model.DownstreamHostAndPorts))
                            {
                                m.DownstreamHostAndPorts =
                                    model.DownstreamHostAndPorts.ToObject<List<FileHostAndPort>>();
                            }

                            if (!string.IsNullOrEmpty(model.RouteClaimsRequirement))
                            {
                                m.RouteClaimsRequirement =
                                    model.RouteClaimsRequirement.ToObject<Dictionary<string, string>>();
                            }

                            //开始赋值
                            m.DownstreamPathTemplate = model.DownstreamPathTemplate;
                            m.DownstreamScheme = model.DownstreamScheme;
                            m.Key = model.Key;
                            m.Priority = model.Priority ?? 0;
                            m.RequestIdKey = model.RequestIdKey;
                            m.ServiceName = model.ServiceName;
                            m.Timeout = model.Timeout ?? 0;
                            m.UpstreamHost = model.UpstreamHost;
                            if (!string.IsNullOrEmpty(model.UpstreamHttpMethod))
                            {
                                m.UpstreamHttpMethod = model.UpstreamHttpMethod.ToObject<List<string>>();
                            }

                            m.UpstreamPathTemplate = model.UpstreamPathTemplate;
                            reroutelist.Add(m);
                        }

                        file.ReRoutes = reroutelist;

                    }
                }
            }
            #endregion

            if (file.ReRoutes == null || file.ReRoutes.Count == 0)
            {
                return new OkResponse<FileConfiguration>(null);
            }

            return new OkResponse<FileConfiguration>(file);

        }

        public Task<Response> Set(FileConfiguration fileConfiguration)
        {
            throw new System.NotImplementedException();
        }
    }
}