namespace TomorrowDAOServer.Dtos.Explorer;

public class ExplorerBalanceRequest
{
    public string Address { get; set; }
}


public class ExplorerBalanceOutput
{
    public string Balance { get; set; }
    public string Symbol { get; set; }
}