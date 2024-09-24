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
    Task GenerateReferralActivityPointsRecordAsync(string chainId, string inviter, string invitee, DateTime voteTime);
    Task GenerateVotePointsRecordAsync(string chainId, string address, DateTime voteTime);
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

    public async Task GenerateReferralActivityPointsRecordAsync(string chainId, string inviter, string invitee, DateTime voteTime)
    {
        var inviterId = GuidHelper.GenerateGrainId(chainId, inviter, invitee, PointsType.InviteVote);
        var inviteeId = GuidHelper.GenerateGrainId(chainId, inviter, invitee, PointsType.BeInviteVote);
        var points = _rankingAppPointsCalcProvider.CalculatePointsFromReferralVotes(1);
        await _userPointsRecordRepository.AddOrUpdateAsync(new UserPointsRecordIndex
        {
            Id = inviterId, ChainId = chainId, Address = inviter, PointsType = PointsType.InviteVote,
            Points = points, PointsTime = voteTime
        });
        await _userPointsRecordRepository.AddOrUpdateAsync(new UserPointsRecordIndex
        {
            Id = inviteeId, ChainId = chainId, Address = invitee, PointsType = PointsType.BeInviteVote,
            Points = points, PointsTime = voteTime
        });
    }

    public async Task GenerateVotePointsRecordAsync(string chainId, string address, DateTime voteTime)
    {
        var id = GuidHelper.GenerateGrainId(chainId, address, TimeHelper.GetTimeStampFromDateTime(voteTime));
        var points = _rankingAppPointsCalcProvider.CalculatePointsFromVotes(1);
        await _userPointsRecordRepository.AddOrUpdateAsync(new UserPointsRecordIndex
        {
            Id = id, ChainId = chainId, Address = address, PointsType = PointsType.Vote,
            Points = points, PointsTime = voteTime
        });
    }
}