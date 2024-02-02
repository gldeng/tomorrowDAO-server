namespace TomorrowDAOServer.Grains.Grain.ApplicationHandler
{
    public interface IBlockchainClientFactory<T> 
        where T : class
    {
        T GetClient(string chainName);
    }
}