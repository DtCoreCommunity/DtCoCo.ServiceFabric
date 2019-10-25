using DtCoCo.ServiceFabric.Exceptionless.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace DtCoCo.ServiceFabric.Exceptionless
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注入服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddExceptionLessService(this IServiceCollection services)
        {
            services.AddSingleton<ILessLog, LessLog>();
            services.AddSingleton<ILessExceptionLog, LessExceptionLog>();
            return services;
        }
    }
}