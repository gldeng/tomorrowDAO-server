using System.Linq;
using System.Threading.Tasks;
using CAServer.Commons;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.AElfSdk;
using TomorrowDAOServer.Common.AElfSdk.Dtos;
using TomorrowDAOServer.Dtos.Explorer;
using TomorrowDAOServer.Dtos.NetworkDao;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.Providers;
using TomorrowDAOServer.Token;
using Volo.Abp.Threading;

namespace TomorrowDAOServer.NetworkDao;

public class TreasuryService : ITreasuryService
{
    private readonly ILogger<TreasuryService> _logger;
    private readonly IClusterClient _clusterClient;
    private readonly IOptionsMonitor<NetworkDaoOptions> _networkDaoOptions;
    private readonly IOptionsMonitor<TokenOptions> _tokenOptions;
    private readonly ITokenService _tokenService;
    private readonly IContractProvider _contractProvider;
    private readonly IExplorerProvider _explorerProvider;

    public TreasuryService(ILogger<TreasuryService> logger, IContractProvider contractProvider,
        IExplorerProvider explorerProvider, ITokenService tokenService, IClusterClient clusterClient,
        IOptionsMonitor<NetworkDaoOptions> networkDaoOptions, IOptionsMonitor<TokenOptions> tokenOptions)
    {
        _logger = logger;
        _contractProvider = contractProvider;
        _explorerProvider = explorerProvider;
        _tokenService = tokenService;
        _clusterClient = clusterClient;
        _networkDaoOptions = networkDaoOptions;
        _tokenOptions = tokenOptions;
    }


    public async Task<TreasuryBalanceResponse> GetBalanceAsync(TreasuryBalanceRequest request)
    {
        var treasuryContractAddress =
            _contractProvider.ContractAddress(request.ChainId, SystemContractName.TreasuryContract);
        var balance = await _explorerProvider.GetBalancesAsync(request.ChainId, new ExplorerBalanceRequest
        {
            Address = treasuryContractAddress
        });

        var balanceItems = balance.Select(b =>
        {
            var token = AsyncHelper.RunSync(() => _tokenService.GetTokenAsync(request.ChainId, b.Symbol));
            var exchange = _networkDaoOptions.CurrentValue.PopularSymbols.Contains(b.Symbol)
                ? AsyncHelper.RunSync(() => _tokenService.GetTokenPriceAsync(b.Symbol, CommonConstant.USDT))
                : null;
            return new TreasuryBalanceResponse.BalanceItem
            {
                TotalCount = b.Balance,
                DollarValue = exchange == null ? null : (b.Balance.SafeToDecimal() * exchange.Price).ToString(2),
                Token = new TreasuryBalanceResponse.TokenItem
                {
                    Symbol = b.Symbol,
                    Name = token.TokenName,
                    Decimals = token.Decimals
                }
            };
        }).ToList();

        return new TreasuryBalanceResponse
        {
            ContractAddress = _contractProvider.ContractAddress(request.ChainId, SystemContractName.TreasuryContract),
            Items = balanceItems
        };
    }
}