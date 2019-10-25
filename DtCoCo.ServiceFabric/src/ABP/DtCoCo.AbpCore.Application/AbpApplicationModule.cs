using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace DtCoCo.AbpCore.Application
{
    [DependsOn(typeof(AbpAutoMapperModule))]
    public class AbpApplicationModule:AbpModule
    {

    }
}