using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.HttpClient;
using TomorrowDAOServer.Dtos.Explorer;
using TomorrowDAOServer.Options;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.Providers;

public interface IExplorerProvider
{
    Task<ExplorerProposalResponse> GetProposalPagerAsync(string chainId, ExplorerProposalListRequest request);
    Task<List<ExplorerBalanceOutput>> GetBalancesAsync(string chainId, ExplorerBalanceRequest request);
    Task<ExplorerTokenInfoResponse> GetTokenInfoAsync(string chainId, ExplorerTokenInfoRequest request);
    Task<ExplorerPagerResult<ExplorerTransactionResponse>> GetTransactionPagerAsync(string chainId,
        ExplorerTransactionRequest request);
}

public static class ExplorerApi
{
    public static readonly ApiInfo ProposalList = new(HttpMethod.Get, "/api/proposal/list");
    public static readonly ApiInfo Organizations = new(HttpMethod.Get, "/api/proposal/organizations");
    public static readonly ApiInfo Balances = new(HttpMethod.Get, "/api/viewer/balances");
    public static readonly ApiInfo TokenInfo = new(HttpMethod.Get, "/api/viewer/tokenInfo");
    public static readonly ApiInfo Transactions = new(HttpMethod.Get, "/api/all/transaction");
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
    public async Task<ExplorerProposalResponse> GetProposalPagerAsync(string chainId,
        ExplorerProposalListRequest request)
    {
        var resp = await _httpProvider.InvokeAsync<ExplorerBaseResponse<ExplorerProposalResponse>>(BaseUrl(chainId),
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
            ExplorerApi.Balances, param: ToDictionary(request));
        AssertHelper.IsTrue(resp.Success, resp.Msg);
        return resp.Data;
    }

    /// <summary>
    ///     Get token info
    /// </summary>
    /// <param name="chainId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ExplorerTokenInfoResponse> GetTokenInfoAsync(string chainId, ExplorerTokenInfoRequest request)
    {
        var resp = await _httpProvider.InvokeAsync<ExplorerBaseResponse<ExplorerTokenInfoResponse>>(BaseUrl(chainId),
            ExplorerApi.TokenInfo, param: ToDictionary(request));
        AssertHelper.IsTrue(resp.Success, resp.Msg);
        return resp.Data;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <returns></returns>
    public async Task<ExplorerPagerResult<ExplorerTransactionResponse>> GetTransactionPagerAsync(string chainId,
        ExplorerTransactionRequest request)
    {
        var resp = await _httpProvider
            .InvokeAsync<ExplorerBaseResponse<ExplorerPagerResult<ExplorerTransactionResponse>>>(BaseUrl(chainId),
                ExplorerApi.Transactions, param: ToDictionary(request));
        AssertHelper.IsTrue(resp.Success, resp.Msg);
        return resp.Data;
    }


    private Dictionary<string, string> ToDictionary(object param)
    {
        if (param == null) return null;
        if (param is Dictionary<string, string>) return param as Dictionary<string, string>;
        var json = param is string ? param as string : JsonConvert.SerializeObject(param);
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
    }
}