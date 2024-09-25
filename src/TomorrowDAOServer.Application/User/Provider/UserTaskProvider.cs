using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Microsoft.Extensions.Logging;
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
    Task BulkAddOrUpdateAsync(List<UserTaskIndex> list);
    Task AddOrUpdateAsync(UserTaskIndex index);
    Task CompleteTaskAsync(string chainId, string address, UserTaskDetail userTaskDetail, DateTime completeTime);
    Task<bool> UpdateUserTaskCompleteTimeAsync(string chainId, string address, UserTask userTask, UserTaskDetail userTaskDetail, DateTime completeTime);
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

    public async Task CompleteTaskAsync(string chainId, string address, UserTaskDetail userTaskDetail, DateTime completeTime)
    {
        var userTask = TaskPointsHelper.GetUserTaskFromUserTaskDetail(userTaskDetail);
        var pointsType = TaskPointsHelper.GetPointsTypeFromUserTaskDetail(userTaskDetail);
        if (userTask == null || pointsType == null)
        {
            return;
        }

        switch (userTask)
        {
            case UserTask.Daily:
                var timeFormat = completeTime.ToUtcString(TimeHelper.DatePattern);
                await _userTaskRepository.AddOrUpdateAsync(new UserTaskIndex
                {
                    Id = GuidHelper.GenerateGrainId(chainId, userTask, userTaskDetail, address, timeFormat),
                    ChainId = chainId, Address = address, UserTask = UserTask.Daily, UserTaskDetail = userTaskDetail,
                    Points = _rankingAppPointsCalcProvider.CalculatePointsFromPointsType(pointsType),
                    CompleteTime = completeTime
                });
                break;
            case UserTask.Explore:
                await _userTaskRepository.AddOrUpdateAsync(new UserTaskIndex
                {
                    Id = GuidHelper.GenerateGrainId(chainId, userTask, userTaskDetail, address),
                    ChainId = chainId, Address = address, UserTask = UserTask.Explore, UserTaskDetail = userTaskDetail,
                    Points = _rankingAppPointsCalcProvider.CalculatePointsFromPointsType(pointsType),
                    CompleteTime = completeTime
                });
                break;
        }
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
}