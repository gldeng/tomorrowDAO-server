
namespace TomorrowDAOServer.Token;

public class TokenGrainDto
{
    public string Id { get; set; }
    public string ChainId { get; set; }
    public string Address { get; set; }
    public string Symbol { get; set; }
    public int Decimals { get; set; }
    public string TokenName { get; set; }
    public string? ImageUrl { get; set; }
    public long LastUpdateTime { get; set; }
    public bool IsNFT => Symbol.Contains("-");
}