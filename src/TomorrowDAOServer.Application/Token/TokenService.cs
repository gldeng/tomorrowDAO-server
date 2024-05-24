using System;
using System.Linq;
using System.Threading.Tasks;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Grains.Grain.Token;
using TomorrowDAOServer.Token.Dto;
using Microsoft.Extensions.Logging;
using Orleans;
using TomorrowDAOServer.Dtos;
using TomorrowDAOServer.Dtos.Explorer;
using TomorrowDAOServer.Providers;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Token;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class TokenService : TomorrowDAOServerAppService, ITokenService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<TokenService> _logger;
    private readonly IExplorerProvider _explorerProvider;
    private readonly IObjectMapper _objectMapper;
    
    
    public TokenService(IClusterClient clusterClient, ILogger<TokenService> logger, IExplorerProvider explorerProvider, IObjectMapper objectMapper)
    {
        _clusterClient = clusterClient;
        _logger = logger;
        _explorerProvider = explorerProvider;
        _objectMapper = objectMapper;
    }

    public async Task<TokenGrainDto> GetTokenAsync(string chainId, string symbol)
    {
        var grainId = GuidHelper.GenerateGrainId(chainId, symbol);

        var tokenGrain = _clusterClient.GetGrain<ITokenGrain>(grainId);

        var grainResultDto = await tokenGrain.GetTokenAsync(new TokenGrainDto
        {
            ChainId = chainId,
            Symbol = symbol
        });
        AssertHelper.IsTrue(grainResultDto.Success, "GetTokenAsync  fail, chainId  {chainId} symbol {symbol}", chainId,
            symbol);
        return grainResultDto.Data;
    }

    public async Task<TokenDto> GetTokenByExplorerAsync(string chainId, string symbol)
    {
        var tokenGrain = await GetTokenAsync(chainId, symbol);
        var tokenInfo = await _explorerProvider.GetTokenInfoAsync(chainId, new ExplorerTokenInfoRequest()
        {
            Symbol = symbol
        });
        var tokenResult = _objectMapper.Map<ExplorerTokenInfoResponse, TokenDto>(tokenInfo);
        tokenResult.ImageUrl = tokenGrain.ImageUrl;
        return tokenResult;
    }
    
    public async Task<TokenPriceDto> GetTokenPriceAsync(string baseCoin, string quoteCoin)
    {
        AssertHelper.IsTrue(!baseCoin.IsNullOrEmpty() && !quoteCoin.IsNullOrEmpty(),
            "Get token price fail, baseCoin or quoteCoin is empty.");
        var pair = string.Join(CommonConstant.Underline, baseCoin, quoteCoin);
        var exchangeGrain = _clusterClient.GetGrain<ITokenExchangeGrain>(pair);
        var exchange = await exchangeGrain.GetAsync();
        AssertHelper.NotEmpty(exchange, "Exchange data not found {}", pair);
        var avgExchange = exchange.Values
            .Where(ex => ex.Exchange > 0)
            .Average(ex => ex.Exchange);
        AssertHelper.IsTrue(avgExchange > 0, "Exchange amount error {avgExchange}", avgExchange);
        return new TokenPriceDto
        {
            Price = avgExchange
        };
    }
}