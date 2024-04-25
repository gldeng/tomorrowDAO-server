using Microsoft.Extensions.DependencyInjection;
using TomorrowDAOServer.Common.Cache;
using TomorrowDAOServer.DAO;
using TomorrowDAOServer.Grains;
using TomorrowDAOServer.Proposal;
using TomorrowDAOServer.Options;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace TomorrowDAOServer;

[DependsOn(
    typeof(TomorrowDAOServerDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(TomorrowDAOServerApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(TomorrowDAOServerGrainsModule),
    typeof(AbpSettingManagementApplicationModule)
)]
public class TomorrowDAOServerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<GraphQLOptions>(configuration.GetSection("GraphQL"));
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<TomorrowDAOServerApplicationModule>(); });
        context.Services.AddTransient<IScheduleSyncDataService, ProposalSyncDataService>();
        context.Services.AddTransient<IScheduleSyncDataService, ProposalUpdateService>();
        context.Services.AddTransient<IScheduleSyncDataService, DAOSyncDataService>();
        context.Services.AddHttpClient();
        context.Services.AddMemoryCache();
        context.Services.AddSingleton(typeof(ILocalMemoryCache<>), typeof(LocalMemoryCache<>));

    }
}
