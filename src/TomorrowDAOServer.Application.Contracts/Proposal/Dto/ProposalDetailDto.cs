using System.Collections.Generic;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Vote.Dto;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalDetailDto : ProposalDto
{
    public List<ProposalLifeDto> ProposalLifeList { get; set; }
    public List<VoteRecordDto> VoteTopList { get; set; }
}