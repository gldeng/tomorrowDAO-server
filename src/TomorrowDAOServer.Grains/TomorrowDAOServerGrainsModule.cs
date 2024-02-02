using AElf.Client.Service;
using TomorrowDAOServer.Grains.Grain.ApplicationHandler;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace TomorrowDAOServer.Grains;

[DependsOn(
    typeof(AbpAutoMapperModule),typeof(TomorrowDAOServerApplicationContractsModule))]
public class TomorrowDAOServerGrainsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<TomorrowDAOServerGrainsModule>(); });
        context.Services.AddSingleton<IBlockchainClientFactory<AElfClient>, AElfClientFactory>();
    }
}