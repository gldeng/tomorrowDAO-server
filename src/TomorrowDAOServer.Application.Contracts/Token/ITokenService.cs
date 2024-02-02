using System.Threading.Tasks;
using TomorrowDAOServer.Token.Dto;

namespace TomorrowDAOServer.Token;

public interface ITokenService
{
    Task<TokenGrainDto> GetTokenAsync(string chainId, string symbol);
    
    Task<TokenPriceDto> GetTokenPriceAsync(string baseCoin, string quoteCoin);
}