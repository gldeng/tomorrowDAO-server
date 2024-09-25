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
using TomorrowDAOServer.User.Dtos;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.User.Provider;

public interface IUserPointsRecordProvider
{
    Task BulkAddOrUpdateAsync(List<UserPointsIndex> list);
    Task GenerateReferralActivityVotePointsRecordAsync(string chainId, string inviter, string invitee, DateTime voteTime);
    Task GenerateVotePointsRecordAsync(string chainId, string address, DateTime voteTime, Dictionary<string, string> information);
    Task GenerateTaskPointsRecordAsync(string chainId, string address, UserTaskDetail userTaskDetail, DateTime completeTime);
    Task<Tuple<long, List<UserPointsIndex>>> GetPointsListAsync(GetMyPointsInput input, string address);
    Task<bool> UpdateUserTaskCompleteTimeAsync(string chainId, string address, UserTask userTask, UserTaskDetail userTaskDetail, DateTime completeTime);
    Task<List<UserPointsIndex>> GetByAddressAndUserTaskAsync(string chainId, string address, UserTask userTask);
}

public class UserPointsRecordProvider : IUserPointsRecordProvider, ISingletonDependency
{
    private readonly INESTRepository<UserPointsIndex, string> _userPointsRecordRepository;
    private readonly IRankingAppPointsCalcProvider _rankingAppPointsCalcProvider;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<UserPointsRecordProvider> _logger;

    public UserPointsRecordProvider(INESTRepository<UserPointsIndex, string> userPointsRecordRepository, 
        IRankingAppPointsCalcProvider rankingAppPointsCalcProvider, IClusterClient clusterClient, ILogger<UserPointsRecordProvider> logger)
    {
        _userPointsRecordRepository = userPointsRecordRepository;
        _rankingAppPointsCalcProvider = rankingAppPointsCalcProvider;
        _clusterClient = clusterClient;
        _logger = logger;
    }

    public async Task BulkAddOrUpdateAsync(List<UserPointsIndex> list)
    {
        if (list == null || list.IsNullOrEmpty())
        {
            return;
        }

        await _userPointsRecordRepository.BulkAddOrUpdateAsync(list);
    }

    public async Task GenerateReferralActivityVotePointsRecordAsync(string chainId, string inviter, string invitee, DateTime voteTime)
    {
        var inviterId = GuidHelper.GenerateGrainId(chainId, inviter, invitee, PointsType.InviteVote);
        var inviteeId = GuidHelper.GenerateGrainId(chainId, inviter, invitee, PointsType.BeInviteVote);
        var points = _rankingAppPointsCalcProvider.CalculatePointsFromReferralVotes(1);
        await _userPointsRecordRepository.AddOrUpdateAsync(new UserPointsIndex
        {
            Id = inviterId, ChainId = chainId, Address = inviter, Information = new Dictionary<string, string>(),
            PointsType = PointsType.InviteVote, Points = points, PointsTime = voteTime
        });
        await _userPointsRecordRepository.AddOrUpdateAsync(new UserPointsIndex
        {
            Id = inviteeId, ChainId = chainId, Address = invitee, Information = new Dictionary<string, string>(),
            PointsType = PointsType.BeInviteVote, Points = points, PointsTime = voteTime
        });
    }

    public async Task GenerateVotePointsRecordAsync(string chainId, string address, DateTime voteTime, Dictionary<string, string> information)
    {
        var id = GuidHelper.GenerateGrainId(chainId, address, voteTime.ToUtcString(TimeHelper.DatePattern));
        var points = _rankingAppPointsCalcProvider.CalculatePointsFromVotes(1);
        await _userPointsRecordRepository.AddOrUpdateAsync(new UserPointsIndex
        {
            Id = id, ChainId = chainId, Address = address, Information = information,
            PointsType = PointsType.Vote, Points = points, PointsTime = voteTime
        });
    }

    public async Task GenerateTaskPointsRecordAsync(string chainId, string address, UserTaskDetail userTaskDetail, DateTime completeTime)
    {
        var userTask = TaskPointsHelper.GetUserTaskFromUserTaskDetail(userTaskDetail);
        var pointsType = TaskPointsHelper.GetPointsTypeFromUserTaskDetail(userTaskDetail);
        if (userTask == null || pointsType == null)
        {
            return;
        }

        var pointsRecordIndex = new UserPointsIndex
        {
            Id = UserTask.Daily == userTask 
                ? GuidHelper.GenerateGrainId(chainId, userTask, userTaskDetail, address, completeTime.ToUtcString(TimeHelper.DatePattern))
                : GuidHelper.GenerateGrainId(chainId, userTask, userTaskDetail, address),
            ChainId = chainId, Address = address, Information = new Dictionary<string, string>(),
            PointsType = pointsType.Value, PointsTime = completeTime,
            Points = _rankingAppPointsCalcProvider.CalculatePointsFromPointsType(pointsType)
        };
        await _userPointsRecordRepository.AddOrUpdateAsync(pointsRecordIndex);
        
    }

    public async Task<Tuple<long, List<UserPointsIndex>>> GetPointsListAsync(GetMyPointsInput input, string address)
    {
        var chainId = input.ChainId;
        var mustQuery = new List<Func<QueryContainerDescriptor<UserPointsIndex>, QueryContainer>>
        {
            q =>
                q.Term(i => i.Field(t => t.ChainId).Value(chainId)),
            q =>
                q.Term(i => i.Field(t => t.Address).Value(address))
        };

        QueryContainer Filter(QueryContainerDescriptor<UserPointsIndex> f) => f.Bool(b => b.Must(mustQuery));
        return await _userPointsRecordRepository.GetSortListAsync(Filter, skip: input.SkipCount, limit: input.MaxResultCount,
            sortFunc: _ => new SortDescriptor<UserPointsIndex>().Descending(index => index.PointsTime));
    }

    public async Task<bool> UpdateUserTaskCompleteTimeAsync(string chainId, string address, UserTask userTask, UserTaskDetail userTaskDetail,
        DateTime completeTime)
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

    public async Task<List<UserPointsIndex>> GetByAddressAndUserTaskAsync(string chainId, string address, UserTask userTask)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserPointsIndex>, QueryContainer>>
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
                .Field(f => f.PointsTime).GreaterThanOrEquals(todayStart).LessThanOrEquals(todayEnd)));
        }

        QueryContainer Filter(QueryContainerDescriptor<UserPointsIndex> f) => f.Bool(b => b.Must(mustQuery));
        return (await _userPointsRecordRepository.GetListAsync(Filter)).Item2;
    }
}