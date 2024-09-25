using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Chains;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.Provider;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.Ranking.Provider;
using TomorrowDAOServer.Referral.Provider;
using TomorrowDAOServer.User;
using TomorrowDAOServer.User.Provider;

namespace TomorrowDAOServer.Referral;

public class ReferralTopInviterGenerateService : ScheduleSyncDataService
{
    private readonly IChainAppService _chainAppService;
    private readonly IOptionsMonitor<RankingOptions> _rankingOptions;
    private readonly IReferralTopInviterProvider _referralTopInviterProvider;
    private readonly IReferralInviteProvider _referralInviteProvider;
    private readonly IUserAppService _userAppService;
    private readonly IRankingAppPointsCalcProvider _rankingAppPointsCalcProvider;
    private readonly IRankingAppPointsRedisProvider _rankingAppPointsRedisProvider;
    private readonly ILogger<ScheduleSyncDataService> _logger;
    private readonly IUserPointsRecordProvider _userPointsRecordProvider;
    
    public ReferralTopInviterGenerateService(ILogger<ReferralTopInviterGenerateService> logger, IGraphQLProvider graphQlProvider, 
        IChainAppService chainAppService, IOptionsMonitor<RankingOptions> rankingOptions, 
        IReferralTopInviterProvider referralTopInviterProvider, IReferralInviteProvider referralInviteProvider, 
        IUserAppService userAppService, IRankingAppPointsCalcProvider rankingAppPointsCalcProvider, 
        IRankingAppPointsRedisProvider rankingAppPointsRedisProvider, IUserPointsRecordProvider userPointsRecordProvider)
        : base(logger, graphQlProvider)
    {
        _logger = logger;
        _chainAppService = chainAppService;
        _rankingOptions = rankingOptions;
        _referralTopInviterProvider = referralTopInviterProvider;
        _referralInviteProvider = referralInviteProvider;
        _userAppService = userAppService;
        _rankingAppPointsCalcProvider = rankingAppPointsCalcProvider;
        _rankingAppPointsRedisProvider = rankingAppPointsRedisProvider;
        _userPointsRecordProvider = userPointsRecordProvider;
    }

    public override async Task<long> SyncIndexerRecordsAsync(string chainId, long lastEndHeight, long newIndexHeight)
    {
        var (latestReferralActiveEnd, latest) = _rankingOptions.CurrentValue.IsLatestReferralActiveEnd();
        if (!latestReferralActiveEnd)
        {
            return -1L;
        }

        var existed = await _referralTopInviterProvider.GetExistByTimeAsync(latest.StartTime, latest.EndTime);
        if (existed)
        {
            return -1L;
        }

        var inviterBuckets = await _referralInviteProvider.InviteLeaderBoardAsync(latest.StartTime, latest.EndTime);
        var caHashList = inviterBuckets.Select(bucket => bucket.Key).Distinct().ToList();
        var userList = await _userAppService.GetUserByCaHashListAsync(caHashList);
        var topList = RankHelper.GetRankedList(chainId, userList, inviterBuckets)
            .Where(referralInvite => referralInvite.Rank is >= 1 and <= 10).ToList();
        _logger.LogInformation("GenerateTopInviterTopList chainId: {chainId} count: {count}", chainId, topList?.Count);
        var toAddTopInviters = new List<ReferralTopInviterIndex>();
        var toAddTopPointRecords = new List<UserPointsIndex>();
        var now = DateTime.Now;
        foreach (var leaderBoardDto in topList)
        {
            var inviter = leaderBoardDto.Inviter;
            var inviterCaHash = leaderBoardDto.InviterCaHash;
            var rank = leaderBoardDto.Rank;
            var inviteAndVoteCount = leaderBoardDto.InviteAndVoteCount;
            toAddTopInviters.Add(new ReferralTopInviterIndex
            {
                Id = GuidHelper.GenerateGrainId(chainId, inviter, 
                    inviterCaHash, latest.StartTime, latest.EndTime),
                ChainId = chainId, InviterCaHash = inviterCaHash,
                InviterAddress = inviter, StartTime = latest.StartTime,
                EndTime = latest.EndTime, Rank = rank,
                InviterCount = inviteAndVoteCount,
                Points = _rankingAppPointsCalcProvider.CalculatePointsFromReferralTopInviter(),
                CreateTime = now
            });
            toAddTopPointRecords.Add(new UserPointsIndex
            {
                Id = GuidHelper.GenerateGrainId(chainId, inviter, 
                    inviterCaHash, latest.StartTime, latest.EndTime, rank, inviteAndVoteCount),
                ChainId = chainId, Address = inviter, PointsType = PointsType.TopInviter,
                Points = _rankingAppPointsCalcProvider.CalculatePointsFromReferralTopInviter(),
                PointsTime = now
            });
            await _rankingAppPointsRedisProvider.IncrementReferralTopInviterPointsAsync(inviter);
        }
        await _referralTopInviterProvider.BulkAddOrUpdateAsync(toAddTopInviters);
        await _userPointsRecordProvider.BulkAddOrUpdateAsync(toAddTopPointRecords);
        return -1L;
    }

    public override async Task<List<string>> GetChainIdsAsync()
    {
        var chainIds = await _chainAppService.GetListAsync();
        return chainIds.ToList();
    }

    public override WorkerBusinessType GetBusinessType()
    {
        return WorkerBusinessType.TopInviterGenerate;
    }
}