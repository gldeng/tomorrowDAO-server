using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TomorrowDAOServer.EntityEventHandler.Core;
using TomorrowDAOServer.Options;
using Volo.Abp.AuditLogging;
using Volo.Abp.AuditLogging.MongoDB;
using Volo.Abp.AutoMapper;
using Volo.Abp.EventBus;
using Volo.Abp.Identity;
using Volo.Abp.Identity.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace TomorrowDAOServer;

[DependsOn(
    typeof(AbpEventBusModule),
    typeof(TomorrowDAOServerApplicationModule),
    typeof(TomorrowDAOServerApplicationContractsModule),
    typeof(TomorrowDAOServerOrleansTestBaseModule),
    typeof(TomorrowDAOServerDomainTestModule)
)]
public class TomorrowDAOServerApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        base.ConfigureServices(context);
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<TomorrowDAOServerApplicationModule>(); });
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<TomorrowDAOServerEntityEventHandlerCoreModule>(); });

        context.Services.AddSingleton(new Mock<IMongoDbContextProvider<IAuditLoggingMongoDbContext>>().Object);
        context.Services.AddSingleton<IAuditLogRepository, MongoAuditLogRepository>();
        context.Services.AddSingleton<IIdentityUserRepository, MongoIdentityUserRepository>();
        
        context.Services.Configure<ProposalTagOptions>(o =>
        {
            o.Mapping = new Dictionary<string, List<string>>
            {
                { "Update Organization",  new List<string>
                {
                    "AddMembers", "ChangeMember", "RemoveMembers"
                } },
                { "DAO Upgrade",  new List<string> { 
                    "UploadFileInfos",
                    "RemoveFileInfos",
                    "SetSubsistStatus",
                    "EnableHighCouncil",
                    "DisableHighCouncil",
                    "HighCouncilConfigSet",
                    "SetPermissions" 
                } 
                },
                { "Customized Vote Model",  new List<string> { 
                    "Parliament",
                    "Association",
                    "Referendum",
                    "Customize"
                } 
                }
            };
        });
    }
}