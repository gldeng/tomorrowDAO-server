using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.HttpClient;
using TomorrowDAOServer.Dtos.Explorer;
using TomorrowDAOServer.Options;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.Providers;

public interface IExplorerProvider
{
    Task<ProposalResponse> GetProposalPagerAsync(string chainId, ProposalListRequest request);
    Task<List<ExplorerBalanceOutput>> GetBalancesAsync(string chainId, ExplorerBalanceRequest request);
}

public static class ExplorerApi
{
    public static ApiInfo ProposalList = new(HttpMethod.Get, "/api/proposal/list");
    public static ApiInfo Organizations = new(HttpMethod.Get, "/api/proposal/organizations");
    public static ApiInfo Balances = new(HttpMethod.Get, "/api/viewer/balances");
}

public class ExplorerProvider : IExplorerProvider, ISingletonDependency
{
    private readonly IHttpProvider _httpProvider;
    private readonly IOptionsMonitor<ExplorerOptions> _explorerOptions;


    public ExplorerProvider(IHttpProvider httpProvider, IOptionsMonitor<ExplorerOptions> explorerOptions)
    {
        _httpProvider = httpProvider;
        _explorerOptions = explorerOptions;
    }


    public string BaseUrl(string chainId)
    {
        var urlExists = _explorerOptions.CurrentValue.BaseUrl.TryGetValue(chainId, out var baseUrl);
        AssertHelper.IsTrue(urlExists, "Explorer url not found of chainId {}", chainId);
        return baseUrl;
    }


    /// <summary>
    ///     GetProposalPagerAsync
    /// </summary>
    /// <param name="chainId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ProposalResponse> GetProposalPagerAsync(string chainId, ProposalListRequest request)
    {
        var resp = await _httpProvider.InvokeAsync<ExplorerBaseResponse<ProposalResponse>>(BaseUrl(chainId),
            ExplorerApi.ProposalList);
        AssertHelper.IsTrue(resp.Success, resp.Msg);
        return resp.Data;
    }


    /// <summary>
    ///     Get Balances by address
    /// </summary>
    /// <param name="chainId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<List<ExplorerBalanceOutput>> GetBalancesAsync(string chainId, ExplorerBalanceRequest request)
    {
        var resp = await _httpProvider.InvokeAsync<ExplorerBaseResponse<List<ExplorerBalanceOutput>>>(BaseUrl(chainId),
            ExplorerApi.Balances, param: new Dictionary<string, string>
            {
                ["address"] = request.Address
            });
        AssertHelper.IsTrue(resp.Success, resp.Msg);
        return resp.Data;
    }
}