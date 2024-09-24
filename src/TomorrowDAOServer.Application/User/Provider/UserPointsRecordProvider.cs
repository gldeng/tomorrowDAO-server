using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Ranking.Provider;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.User.Provider;

public interface IUserPointsRecordProvider
{
    Task BulkAddOrUpdateAsync(List<UserPointsRecordIndex> list);
    Task AddOrUpdateAsync(UserPointsRecordIndex index);
    Task GenerateReferralActivityVotePointsRecordAsync(string chainId, string inviter, string invitee, DateTime voteTime);
    Task GenerateVotePointsRecordAsync(string chainId, string address, Dictionary<string, string> information, DateTime voteTime);
    Task GenerateTaskPointsRecordAsync(string chainId, string address, Dictionary<string, string> information, UserTaskDetail userTaskDetail, DateTime completeTime);
}

public class UserPointsRecordProvider : IUserPointsRecordProvider, ISingletonDependency
{
    private readonly INESTRepository<UserPointsRecordIndex, string> _userPointsRecordRepository;
    private readonly IRankingAppPointsCalcProvider _rankingAppPointsCalcProvider;

    public UserPointsRecordProvider(INESTRepository<UserPointsRecordIndex, string> userPointsRecordRepository, 
        IRankingAppPointsCalcProvider rankingAppPointsCalcProvider)
    {
        _userPointsRecordRepository = userPointsRecordRepository;
        _rankingAppPointsCalcProvider = rankingAppPointsCalcProvider;
    }

    public async Task BulkAddOrUpdateAsync(List<UserPointsRecordIndex> list)
    {
        if (list == null || list.IsNullOrEmpty())
        {
            return;
        }

        await _userPointsRecordRepository.BulkAddOrUpdateAsync(list);
    }

    public async Task AddOrUpdateAsync(UserPointsRecordIndex index)
    {
        if (index == null)
        {
            return;
        }
        await _userPointsRecordRepository.AddOrUpdateAsync(index);
    }

    public async Task GenerateReferralActivityVotePointsRecordAsync(string chainId, string inviter, string invitee, DateTime voteTime)
    {
        var inviterId = GuidHelper.GenerateGrainId(chainId, inviter, invitee, PointsType.InviteVote);
        var inviteeId = GuidHelper.GenerateGrainId(chainId, inviter, invitee, PointsType.BeInviteVote);
        var points = _rankingAppPointsCalcProvider.CalculatePointsFromReferralVotes(1);
        await _userPointsRecordRepository.AddOrUpdateAsync(new UserPointsRecordIndex
        {
            Id = inviterId, ChainId = chainId, Address = inviter, Information = new Dictionary<string, string>(),
            PointsType = PointsType.InviteVote, Points = points, PointsTime = voteTime
        });
        await _userPointsRecordRepository.AddOrUpdateAsync(new UserPointsRecordIndex
        {
            Id = inviteeId, ChainId = chainId, Address = invitee, Information = new Dictionary<string, string>(),
            PointsType = PointsType.BeInviteVote, Points = points, PointsTime = voteTime
        });
    }

    public async Task GenerateVotePointsRecordAsync(string chainId, string address, Dictionary<string, string> information, DateTime voteTime)
    {
        var id = GuidHelper.GenerateGrainId(chainId, address, TimeHelper.GetTimeStampFromDateTime(voteTime));
        var points = _rankingAppPointsCalcProvider.CalculatePointsFromVotes(1);
        await _userPointsRecordRepository.AddOrUpdateAsync(new UserPointsRecordIndex
        {
            Id = id, ChainId = chainId, Address = address, Information = information,
            PointsType = PointsType.Vote, Points = points, PointsTime = voteTime
        });
    }

    public async Task GenerateTaskPointsRecordAsync(string chainId, string address, Dictionary<string, string> information, UserTaskDetail userTaskDetail,
        DateTime completeTime)
    {
        var userTask = TaskPointsHelper.GetUserTaskFromUserTaskDetail(userTaskDetail);
        var pointsType = TaskPointsHelper.GetPointsTypeFromUserTaskDetail(userTaskDetail);
        if (userTask == null || pointsType == null)
        {
            return;
        }

        var pointsRecordIndex = new UserPointsRecordIndex
        {
            ChainId = chainId, Address = address, Information = information,
            PointsType = pointsType.Value, PointsTime = completeTime,
            Points = _rankingAppPointsCalcProvider.CalculatePointsFromPointsType(pointsType)
        };
        switch (userTask)
        {
            case UserTask.Daily:
                var timeFormat = completeTime.ToUtcString(TimeHelper.DatePattern);
                pointsRecordIndex.Id = GuidHelper.GenerateGrainId(chainId, userTask, userTaskDetail, address, timeFormat);
                await _userPointsRecordRepository.AddOrUpdateAsync(pointsRecordIndex);
                break;
            case UserTask.Explore:
                pointsRecordIndex.Id = GuidHelper.GenerateGrainId(chainId, userTask, userTaskDetail, address);
                await _userPointsRecordRepository.AddOrUpdateAsync(pointsRecordIndex);
                break;
        }
        
    }
}