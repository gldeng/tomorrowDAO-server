using TomorrowDAOServer.Token;

namespace TomorrowDAOServer.Grains.State.Token;

public class TokenExchangeState
{
    public long LastModifyTime { get; set; }
    public long ExpireTime { get; set; }
    public Dictionary<string, TokenExchangeDto> ExchangeInfos { get; set; }
}