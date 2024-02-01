using System.Collections.Concurrent;
using AElf.Client.Service;
using TomorrowDAOServer.EntityEventHandler.Core.Background.Options;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.EntityEventHandler.Core.Background.Providers.Impl;

public class AElfClientProvider : IAElfClientProvider, ISingletonDependency
{
    private readonly ApiOptions _apiOptions;
    private readonly ConcurrentDictionary<string, AElfClient> _clientDic;
    
    public AElfClientProvider(IOptionsSnapshot<ApiOptions> apiOptions)
    {
        _apiOptions = apiOptions.Value;
        _clientDic = new ConcurrentDictionary<string, AElfClient>();
    }
    
    public AElfClient GetClient(string chainName)
    {
        if (_clientDic.TryGetValue(chainName, out var client))
        {
            return client;
        }
    
        _clientDic[chainName] = new AElfClient(_apiOptions.ChainNodeApis[chainName]);
        return _clientDic[chainName];
    }
}