using AElf.Contracts.MultiToken;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Grains.State.Token;
using TomorrowDAOServer.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using TomorrowDAOServer.Common.AElfSdk;
using TomorrowDAOServer.Common.AElfSdk.Dtos;
using TomorrowDAOServer.Common.Aws;
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
    private readonly IOptionsMonitor<ChainOptions> _chainOptions;
    private readonly IObjectMapper _objectMapper;
    private readonly IContractProvider _contractProvider;
    private readonly IAwsS3Client _awsS3Client;


    public TokenGrain(ILogger<TokenGrain> logger, IObjectMapper objectMapper,
        IOptionsMonitor<ChainOptions> chainOptionsMonitor,
        IContractProvider contractProvider, IAwsS3Client awsS3Client)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _chainOptions = chainOptionsMonitor;
        _contractProvider = contractProvider;
        _awsS3Client = awsS3Client;
    }

    public async Task<GrainResultDto<TokenGrainDto>> GetTokenAsync(TokenGrainDto input)
    {
        if (!State.Id.IsNullOrEmpty())
        {
            if (State.ImageUrl.IsNullOrEmpty())
            {
                await FixImageAsync(State.ChainId, State.Symbol);
                if (State.ImageUrl.NotNullOrEmpty())
                {
                    State.LastUpdateTime = DateTime.UtcNow.ToUtcMilliSeconds();
                    _logger.LogInformation(
                        "[TokenGrain] Fix images chainId={ChainId}, symbol={Symbol}, decimals={Decimals}, tokenName={Name}, img={Img}",
                        State.ChainId, State.Symbol, State.Decimals, State.TokenName, State.ImageUrl);
                    await WriteStateAsync();
                }
            }

            return GrainDtoBuilder();
        }

        State = _objectMapper.Map<TokenGrainDto, TokenState>(input);

        var tokenInfo = await GetBlockChainTokenInfo(input.ChainId, State.Symbol);
        await FixImageAsync(State.ChainId, State.Symbol, tokenInfo);
        State.Id = GuidHelper.GetTokenInfoId(State.ChainId, State.Symbol);
        State.Decimals = tokenInfo.Decimals;
        State.TokenName = tokenInfo.TokenName;
        State.LastUpdateTime = DateTime.UtcNow.ToUtcMilliSeconds();
        _logger.LogInformation("[TokenGrain] get token info chainId {} symbol {} decimals {} tokenName {}",
            State.ChainId, State.Symbol, State.Decimals, State.TokenName);
        await WriteStateAsync();

        return GrainDtoBuilder();
    }

    private async Task FixImageAsync(string chainId, string symbol, TokenInfo tokenInfo = null)
    {
        if (State.ImageUrl.NotNullOrEmpty() || !State.IsNFT) return;
        if (State.LastUpdateTime + _chainOptions.CurrentValue.TokenImageRefreshDelaySeconds * 1000 >
            DateTime.UtcNow.ToUtcMilliSeconds()) return;
        try
        {
            tokenInfo ??= await GetBlockChainTokenInfo(chainId, symbol);
            var externalInfo = tokenInfo.ExternalInfo.Value.ToDictionary(f => f.Key, f => f.Value);
            if (externalInfo.TryGetValue("__nft_image_url", out var nftImage))
            {
                // for common nft, save image url directly
                State.ImageUrl = nftImage;
            }
            else if (externalInfo.TryGetValue("inscription_image", out var inscriptionImage))
            {
                // for inscription nft, upload image to AwsS3 and save image url
                State.ImageUrl = await _awsS3Client.UpLoadBase64FileAsync(inscriptionImage, State.Symbol + ".png");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fix token image ERROR, chainId {} symbol {} decimals {} tokenName {}",
                State.ChainId, State.Symbol, State.Decimals, State.TokenName);
        }
    }

    private async Task<TokenInfo> GetBlockChainTokenInfo(string chainId, string symbol)
    {
        var (_, tx) = await _contractProvider.CreateCallTransactionAsync(chainId,
            SystemContractName.TokenContract, "GetTokenInfo", new GetTokenInfoInput { Symbol = symbol });
        var tokenInfo = await _contractProvider.CallTransactionAsync<TokenInfo>(chainId, tx);
        return tokenInfo;
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