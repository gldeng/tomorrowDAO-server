using System.Collections.Generic;
using TomorrowDAOServer.Common;

namespace TomorrowDAOServer.Options;

public class NetworkDaoOptions
{
    
    public int CurrentTermMiningRewardCacheSeconds { get; set; } = 60;
    public int ProposalVoteCountCacheSeconds { get; set; } = 60;

    public List<string> PopularSymbols { get; set; } = new()
    {
        CommonConstant.ELF, CommonConstant.USDT
    };

}