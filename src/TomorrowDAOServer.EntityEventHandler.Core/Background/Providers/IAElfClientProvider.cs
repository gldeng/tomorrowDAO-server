using AElf.Client.Service;

namespace TomorrowDAOServer.EntityEventHandler.Core.Background.Providers
{
    public interface IAElfClientProvider
    {
        AElfClient GetClient(string chainName);
    }
}