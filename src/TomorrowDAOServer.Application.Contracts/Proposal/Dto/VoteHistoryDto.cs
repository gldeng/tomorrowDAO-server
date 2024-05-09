using System;
using System.Collections.Generic;
using TomorrowDAOServer.Enums;

namespace TomorrowDAOServer.Proposal.Dto;

public class VoteHistoryDto
{
    public string ChainId { get; set; }
    public List<IndexerVoteHistoryDto> Items { get; set; }
}

public class IndexerVoteHistoryDto
{
    public DateTime TimeStamp { get; set; }
    public string ProposalId { get; set; }
    public string ProposalTitle { get; set; }
    public VoteOption MyOption { get; set; }
    public int VoteNum { get; set; }
    public string TransactionId { get; set; }
    public string Executer { get; set; }
}