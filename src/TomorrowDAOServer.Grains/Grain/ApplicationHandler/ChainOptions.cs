namespace TomorrowDAOServer.Grains.Grain.ApplicationHandler;

public class ChainOptions
{
    public Dictionary<string, ChainInfo> ChainInfos { get; set; }
}


public class ChainInfo
{
    public string ChainId { get; set; }
    public string BaseUrl { get; set; }
    
    public string PrivateKey { get; set; }

    public string TokenContractAddress { get; set; }
    
    public string IdoContractAddress { get; set; }
    
    public string WhitelistContractAddress { get; set; }
}