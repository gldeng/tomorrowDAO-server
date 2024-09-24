using Orleans;
using TomorrowDAOServer.Grains.State.Users;

namespace TomorrowDAOServer.Grains.Grain.Users;

public interface IUserTaskGrain : IGrainWithStringKey
{
    Task<long> GetUserTaskCompleteTimeAsync();
}

public class UserTaskGrain : Grain<UserTaskState>, IUserTaskGrain
{
    public async Task<long> GetUserTaskCompleteTimeAsync()
    {
        var lastCompleteTime = State.CompleteTime;
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var completeDate = DateTimeOffset.FromUnixTimeMilliseconds(lastCompleteTime).Date;
        var currentDate = DateTime.UtcNow.Date;

        if (completeDate == currentDate)
        {
            return -1L;
        }

        State.CompleteTime = currentTime;
        await WriteStateAsync();
        return currentTime;
    }
}