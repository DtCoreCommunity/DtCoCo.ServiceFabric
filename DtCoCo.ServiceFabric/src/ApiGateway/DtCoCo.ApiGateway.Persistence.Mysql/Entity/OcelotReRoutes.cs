namespace DtCoCo.Ocelot.Persistence.Mysql.Entity
{

    public class OcelotReRoutes
    {
        public virtual int Id { get; set; }

        public virtual int OcelotGlobalConfigurationId { get; set; }

        public virtual string UpstreamPathTemplate { get; set; }

        public virtual string UpstreamHttpMethod { get; set; }

        public virtual string UpstreamHost { get; set; }

        public virtual string DownstreamScheme { get; set; }

        public virtual string DownstreamPathTemplate { get; set; }

        public virtual string DownstreamHostAndPorts { get; set; }

        public virtual string AuthenticationOptions { get; set; }

        public virtual string RequestIdKey { get; set; }

        public virtual string CacheOptions { get; set; }

        public virtual string ServiceName { get; set; }

        public virtual string QoSOptions { get; set; }

        public virtual string LoadBalancerOptions { get; set; }

        public virtual string RouteClaimsRequirement { get; set; }

        public virtual string Key { get; set; }

        public virtual string DelegatingHandlers { get; set; }

        public virtual int? Priority { get; set; }

        public virtual int? Timeout { get; set; }

        public virtual int Status { get; set; }
    }
}