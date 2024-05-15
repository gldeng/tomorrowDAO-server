using System.Collections.Generic;

namespace TomorrowDAOServer.Vote.Dto;

public class GetAllNonWithdrawVoteRecordInput
{
    public string ChainId { get; set; }

    public string DAOId { get; set; }
    
    public string Voter { get; set; }
    
    public List<string> WithdrawVotingItemIds { get; set; }
}