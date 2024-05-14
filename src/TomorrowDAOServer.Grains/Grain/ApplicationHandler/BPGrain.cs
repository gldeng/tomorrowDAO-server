using TomorrowDAOServer.Grains.State.ApplicationHandler;
using Orleans;

namespace TomorrowDAOServer.Grains.Grain.ApplicationHandler;

public interface IBPGrain : IGrainWithStringKey
{
    Task SetBPAsync(List<string> addressList);
    Task<List<string>> GetBPAsync();
}

public class BPGrain : Grain<BPState>, IBPGrain
{
    public override Task OnActivateAsync()
    {
        ReadStateAsync();
        return base.OnActivateAsync();
    }

    public async Task SetBPAsync(List<string> addressList)
    {
        State.AddressList = addressList;
        await WriteStateAsync();
    }

    public Task<List<string>> GetBPAsync()
    {
        return Task.FromResult(State.AddressList);
    }
}