namespace DtCoCo.Ocelot.Persistence.Mysql.Entity
{
    public class OcelotGlobalConfiguration
    {
        public virtual int Id { get; set; }

        public virtual string GatewayName { get; set; }

        public virtual string RequestIdKey { get; set; }

        public virtual string BaseUrl { get; set; }

        public virtual string DownstreamScheme { get; set; }

        public virtual string ServiceDiscoveryProvider { get; set; }

        public virtual string QoSOptions { get; set; }

        public virtual string LoadBalancerOptions { get; set; }

        public virtual string HttpHandlerOptions { get; set; }
    }
}