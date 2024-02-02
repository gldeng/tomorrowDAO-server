using AElf;
using AElf.Client.Dto;
using AElf.Client.MultiToken;
using AElf.Client.Service;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Grains.Grain.ApplicationHandler;
using TomorrowDAOServer.Grains.State.Token;
using TomorrowDAOServer.Token;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Volo.Abp.ObjectMapping;

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


    public TokenGrain( ILogger<TokenGrain> logger, IObjectMapper objectMapper, 
        IOptionsMonitor<ChainOptions> chainOptionsMonitor, 
        IBlockchainClientFactory<AElfClient> blockchainClientFactory)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _chainOptionsMonitor = chainOptionsMonitor;
        _blockchainClientFactory = blockchainClientFactory;
    }

    public async Task<GrainResultDto<TokenGrainDto>> GetTokenAsync(TokenGrainDto input)
    {
        if (!State.Id.IsNullOrEmpty())
        {
            return GrainDtoBuilder();
        }
        State = _objectMapper.Map<TokenGrainDto, TokenState>(input);
        var chainOptions = _chainOptionsMonitor.CurrentValue;
        var client = _blockchainClientFactory.GetClient(State.ChainId);
        var privateKey = chainOptions.ChainInfos[State.ChainId].PrivateKey;
        var address = chainOptions.ChainInfos[State.ChainId].TokenContractAddress;
        var transactionGetToken =
            await client.GenerateTransactionAsync(client.GetAddressFromPrivateKey(privateKey), address,
                "GetTokenInfo", new GetTokenInfoInput { Symbol = State.Symbol });
        var txWithSignGetToken = client.SignTransaction(privateKey, transactionGetToken);
        var transactionGetTokenResult = await client.ExecuteTransactionAsync(new ExecuteTransactionDto
        {
            RawTransaction = txWithSignGetToken.ToByteArray().ToHex()
        });
        var token = TokenInfo.Parser.ParseFrom(
            ByteArrayHelper.HexStringToByteArray(transactionGetTokenResult));
        State.Id = GuidHelper.GetTokenInfoId(State.ChainId, State.Symbol);
        State.Address = address;
        State.Decimals = token.Decimals;
        State.TokenName = token.TokenName;

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