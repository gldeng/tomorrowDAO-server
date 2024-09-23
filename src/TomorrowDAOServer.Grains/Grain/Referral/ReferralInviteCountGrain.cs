using Orleans;
using TomorrowDAOServer.Grains.State.Referral;

namespace TomorrowDAOServer.Grains.Grain.Referral;

public interface IReferralInviteCountGrain : IGrainWithStringKey
{
    Task<long> GetInviteCount();
}

public class ReferralInviteCountGrain : Grain<InviteCountState>, IReferralInviteCountGrain
{
    public override Task OnActivateAsync()
    {
        ReadStateAsync();
        return base.OnActivateAsync();
    }

    public async Task<long> GetInviteCount()
    {
        State.InviteCount++;
        await WriteStateAsync();
        return State.InviteCount;
    }
}