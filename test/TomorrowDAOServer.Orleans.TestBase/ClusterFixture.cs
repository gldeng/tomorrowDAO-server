using System;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.TestingHost;
using TomorrowDAOServer.Grains;
using Volo.Abp.AutoMapper;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Reflection;

namespace TomorrowDAOServer;

public class ClusterFixture : IDisposable, ISingletonDependency
{
    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        var randomPort = DateTime.UtcNow.Second * 1000 + DateTime.UtcNow.Millisecond;
        builder.Options.BaseGatewayPort = 2000 + randomPort;
        builder.Options.BaseSiloPort = 1000 + randomPort;
        builder.Options.InitialSilosCount = 1;

        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        // builder.AddClientBuilderConfigurator<TestClientBuilderConfigurator>();
        Cluster = builder.Build();
        var retryCount = 30;
        while (true)
        {
            try
            {
                Cluster.Deploy();
                break;
            } 
            catch (Exception ex)
            {
                builder.Options.BaseGatewayPort++;
                builder.Options.BaseSiloPort++;
                Cluster = builder.Build();
                if (retryCount-- <= 0)
                {
                    throw;
                }
            }
        }
    }

    public void Dispose()
    {
        Cluster.StopAllSilos();
    }

    public TestCluster Cluster { get; private set; }


    private class TestSiloConfigurations : ISiloBuilderConfigurator
    {
        public void Configure(ISiloHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
                {
                    services.AddMemoryCache();
                    services.AddDistributedMemoryCache();
                    services.AddAutoMapper(typeof(TomorrowDAOServerGrainsModule).Assembly);

                    services.AddSingleton(typeof(IDistributedCache), typeof(MemoryDistributedCache));
                    // services.AddSingleton(typeof(IDistributedCache<>), typeof(MemoryDistributedCache<>));
                    services.AddSingleton(typeof(IDistributedCache<,>), typeof(DistributedCache<,>));

                    services.Configure<AbpDistributedCacheOptions>(cacheOptions =>
                    {
                        cacheOptions.GlobalCacheEntryOptions.SlidingExpiration = TimeSpan.FromMinutes(20);
                    });
                    services.OnExposing(onServiceExposingContext =>
                    {
                        //Register types for IObjectMapper<TSource, TDestination> if implements
                        onServiceExposingContext.ExposedTypes.AddRange(
                            ReflectionHelper.GetImplementedGenericTypes(
                                onServiceExposingContext.ImplementationType,
                                typeof(IObjectMapper<,>)
                            )
                        );
                    });
                    services.AddTransient(
                        typeof(IObjectMapper<>),
                        typeof(DefaultObjectMapper<>)
                    );
                    services.AddTransient(
                        typeof(IObjectMapper),
                        typeof(DefaultObjectMapper)
                    );
                    services.AddTransient(typeof(IAutoObjectMappingProvider),
                        typeof(AutoMapperAutoObjectMappingProvider));
                    services.AddTransient(sp => new MapperAccessor()
                    {
                        Mapper = sp.GetRequiredService<IMapper>()
                    });
                    services.AddTransient<IMapperAccessor>(provider => provider.GetRequiredService<MapperAccessor>());
                })
                .AddMemoryGrainStorage("PubSubStore")
                .AddMemoryGrainStorageAsDefault();
        }
    }

    public class MapperAccessor : IMapperAccessor
    {
        public IMapper Mapper { get; set; }
    }

}