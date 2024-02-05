using AElf.Client;
using AElf.Contracts.MultiToken;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Grains.Grain.ApplicationHandler;
using TomorrowDAOServer.Grains.State.Token;
using TomorrowDAOServer.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using TomorrowDAOServer.Common.AElfSdk;
using TomorrowDAOServer.Common.AElfSdk.Dtos;
using TomorrowDAOServer.Options;
using Volo.Abp.ObjectMapping;
using TokenInfo = AElf.Contracts.MultiToken.TokenInfo;

namespace TomorrowDAOServer.Grains.Grain.Token;

public interface ITokenGrain : IGrainWithStringKey
{
    Task<GrainResultDto<TokenGrainDto>> GetTokenAsync(TokenGrainDto input);
}

public class TokenGrain : Grain<TokenState>, ITokenGrain
{
    private readonly ILogger<TokenGrain> _logger;
    private readonly IOptionsMonitor<ChainOptions> _chainOptionsMonitor;
    private readonly IObjectMapper _objectMapper;
    private readonly IBlockchainClientFactory<AElfClient> _blockchainClientFactory;
    private readonly IContractProvider _contractProvider;


    public TokenGrain(ILogger<TokenGrain> logger, IObjectMapper objectMapper,
        IOptionsMonitor<ChainOptions> chainOptionsMonitor,
        IBlockchainClientFactory<AElfClient> blockchainClientFactory, IContractProvider contractProvider)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _chainOptionsMonitor = chainOptionsMonitor;
        _blockchainClientFactory = blockchainClientFactory;
        _contractProvider = contractProvider;
    }

    public async Task<GrainResultDto<TokenGrainDto>> GetTokenAsync(TokenGrainDto input)
    {
        if (!State.Id.IsNullOrEmpty())
        {
            return GrainDtoBuilder();
        }
        State = _objectMapper.Map<TokenGrainDto, TokenState>(input);
        
        var (_, tx) = await _contractProvider.CreateCallTransactionAsync(input.ChainId,
            SystemContractName.TokenContract, "GetTokenInfo", new GetTokenInfoInput { Symbol = State.Symbol });

        var transactionGetTokenResult = await _contractProvider.CallTransactionAsync<TokenInfo>(input.ChainId, tx);
        State.Id = GuidHelper.GetTokenInfoId(State.ChainId, State.Symbol);
        State.Decimals = transactionGetTokenResult.Decimals;
        State.TokenName = transactionGetTokenResult.TokenName;

        _logger.LogInformation("[TokenGrain] get token info chainId {} symbol {} decimals {} tokenName {}",
            State.ChainId, State.Symbol, State.Decimals, State.TokenName);
        await WriteStateAsync();

        return GrainDtoBuilder();
    }

    private GrainResultDto<TokenGrainDto> GrainDtoBuilder()
    {
        return new GrainResultDto<TokenGrainDto>()
        {
            Success = true,
            Data = _objectMapper.Map<TokenState, TokenGrainDto>(State)
        };
    }
}