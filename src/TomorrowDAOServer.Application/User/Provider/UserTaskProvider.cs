using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Microsoft.Extensions.Logging;
using Nest;
using Orleans;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Grains.Grain.Users;
using TomorrowDAOServer.Ranking.Provider;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.User.Provider;

public interface IUserTaskProvider
{
    Task CompleteTaskAsync(string chainId, string address, UserTaskDetail userTaskDetail, DateTime completeTime);
    Task<bool> UpdateUserTaskCompleteTimeAsync(string chainId, string address, UserTask userTask, UserTaskDetail userTaskDetail, DateTime completeTime);
    Task<List<UserTaskIndex>> GetByAddressAndUserTaskAsync(string chainId, string address, UserTask userTask);
}

public class UserTaskProvider : IUserTaskProvider, ISingletonDependency
{
    private readonly INESTRepository<UserTaskIndex, string> _userTaskRepository;
    private readonly IRankingAppPointsCalcProvider _rankingAppPointsCalcProvider;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<UserTaskProvider> _logger;

    public UserTaskProvider(INESTRepository<UserTaskIndex, string> userTaskRepository, 
        IRankingAppPointsCalcProvider rankingAppPointsCalcProvider, IClusterClient clusterClient, 
        ILogger<UserTaskProvider> logger)
    {
        _userTaskRepository = userTaskRepository;
        _rankingAppPointsCalcProvider = rankingAppPointsCalcProvider;
        _clusterClient = clusterClient;
        _logger = logger;
    }

    public async Task CompleteTaskAsync(string chainId, string address, UserTaskDetail userTaskDetail, DateTime completeTime)
    {
        var userTask = TaskPointsHelper.GetUserTaskFromUserTaskDetail(userTaskDetail);
        var pointsType = TaskPointsHelper.GetPointsTypeFromUserTaskDetail(userTaskDetail);
        if (userTask == null || pointsType == null)
        {
            return;
        }

        var userTaskIndex = new UserTaskIndex
        {
            Id = UserTask.Daily == userTask 
                ? GuidHelper.GenerateGrainId(chainId, userTask, userTaskDetail, address, completeTime.ToUtcString(TimeHelper.DatePattern))
                : GuidHelper.GenerateGrainId(chainId, userTask, userTaskDetail, address),
            ChainId = chainId, Address = address, UserTask = userTask.Value, UserTaskDetail = userTaskDetail,
            Points = _rankingAppPointsCalcProvider.CalculatePointsFromPointsType(pointsType),
            CompleteTime = completeTime
        };
        await _userTaskRepository.AddOrUpdateAsync(userTaskIndex);
    }

    public async Task<bool> UpdateUserTaskCompleteTimeAsync(string chainId, string address, UserTask userTask, UserTaskDetail userTaskDetail, DateTime completeTime)
    {
        var id = GuidHelper.GenerateGrainId(chainId, userTask, userTaskDetail, address);
        try
        {
            var grain = _clusterClient.GetGrain<IUserTaskGrain>(id);
            return await grain.UpdateUserTaskCompleteTimeAsync(completeTime, userTask);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetUserTaskCompleteTimeAsyncException id {id}", id);
            return false;
        }
    }

    public async Task<List<UserTaskIndex>> GetByAddressAndUserTaskAsync(string chainId, string address, UserTask userTask)
    {
        var timeFormat = DateTime.UtcNow.ToUtcString(TimeHelper.DatePattern);
        var mustQuery = new List<Func<QueryContainerDescriptor<UserTaskIndex>, QueryContainer>>
        {
            q => q.Term(i => i.Field(t => t.ChainId).Value(chainId)),
            q => q.Term(i => i.Field(t => t.Address).Value(address)),
            q => q.Term(i => i.Field(t => t.UserTask).Value(userTask))
        };
        if (userTask == UserTask.Daily)
        {
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1).AddTicks(-1); 
            mustQuery.Add(q => q.DateRange(r => r
                .Field(f => f.CompleteTime).GreaterThanOrEquals(todayStart).LessThanOrEquals(todayEnd)));
        }

        QueryContainer Filter(QueryContainerDescriptor<UserTaskIndex> f) => f.Bool(b => b.Must(mustQuery));
        return (await _userTaskRepository.GetListAsync(Filter)).Item2;
    }
}