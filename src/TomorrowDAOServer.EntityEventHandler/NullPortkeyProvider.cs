using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Providers;
using TomorrowDAOServer.Referral.Dto;
using TomorrowDAOServer.Referral.Indexer;

namespace TomorrowDAOServer.EntityEventHandler;

public class NullPortkeyProvider:IPortkeyProvider
{
    public Task<Tuple<string, string>> GetShortLinkAsync(string chainId, string token)
    {
        return Task.FromResult( new Tuple<string,string>("", ""));
    }

    public Task<List<IndexerReferral>> GetSyncReferralListAsync(string methodName, long startTime, long endTime, int skipCount, int maxResultCount)
    {
        return Task.FromResult(new List<IndexerReferral>());
    }

    public Task<List<ReferralCodeInfo>> GetReferralCodeCaHashAsync(List<string> referralCodes)
    {
        return Task.FromResult<List<ReferralCodeInfo>>(new List<ReferralCodeInfo>());
    }

    public Task<List<CaHolderTransactionDetail>> GetCaHolderTransactionAsync(string chainId, string caAddress)
    {
        return Task.FromResult(new List<CaHolderTransactionDetail>());
    }

    public Task<HolderInfoIndexerDto> GetHolderInfosAsync(string caHash)
    {
        return Task.FromResult(new HolderInfoIndexerDto());
    }
}