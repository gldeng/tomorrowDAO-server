namespace TomorrowDAOServer.Token;

public interface IToken
{
    public string Symbol { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public int Decimals { get; set; }
}