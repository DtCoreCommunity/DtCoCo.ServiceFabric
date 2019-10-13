namespace DtCoCo.Ocelot.Extension.Configurations
{
    /// <summary>
    /// 重写管道需要传入的配置项
    /// </summary>
    public class ConfigAuthLimitCacheOptions
    {
        /// <summary>
        /// 提取数据缓存过期时间(秒),默认1个小时
        /// </summary>
        public int CacheExpireTime { get; set; } = 3600;

        /// <summary>
        /// 缓存默认前缀，防止重复
        /// </summary>
        public string CachePrefix { get; set; } = "oce";

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string DbConnectionStrings { get; set; }
    }
}