using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.User.Provider;

public interface IUserTaskProvider
{
    Task BulkAddOrUpdateAsync(List<UserTaskIndex> list);
    Task AddOrUpdateAsync(UserTaskIndex index);
    Task GenerateCompleteTaskAsync(string chainId, string address, UserTaskDetail userTaskDetail);
}

public class UserTaskProvider : IUserTaskProvider, ISingletonDependency
{
    private readonly INESTRepository<UserTaskIndex, string> _userTaskRepository;

    public UserTaskProvider(INESTRepository<UserTaskIndex, string> userTaskRepository)
    {
        _userTaskRepository = userTaskRepository;
    }

    public async Task BulkAddOrUpdateAsync(List<UserTaskIndex> list)
    {
        if (list == null || list.IsNullOrEmpty())
        {
            return;
        }

        await _userTaskRepository.BulkAddOrUpdateAsync(list);
    }

    public async Task AddOrUpdateAsync(UserTaskIndex index)
    {
        if (index == null)
        {
            return;
        }
        await _userTaskRepository.AddOrUpdateAsync(index);
    }

    public Task GenerateCompleteTaskAsync(string chainId, string address, UserTaskDetail userTaskDetail)
    {
        throw new System.NotImplementedException();
    }
}