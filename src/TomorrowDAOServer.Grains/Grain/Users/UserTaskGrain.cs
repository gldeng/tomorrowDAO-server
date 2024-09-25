using Orleans;
using TomorrowDAOServer.Grains.State.Users;

namespace TomorrowDAOServer.Grains.Grain.Users;

public interface IUserTaskGrain : IGrainWithStringKey
{
    Task<bool> UpdateUserTaskCompleteTimeAsync(DateTime completeTime);
}

public class UserTaskGrain : Grain<UserTaskState>, IUserTaskGrain
{
    public async Task<bool> UpdateUserTaskCompleteTimeAsync(DateTime completeTime)
    {
        var lastCompleteTime = State.CompleteTime;
        if (completeTime <= lastCompleteTime || completeTime.Date == lastCompleteTime.Date)
        {
            return false;
        }

        State.CompleteTime = completeTime;
        await WriteStateAsync(); 
        return true;

    }
}