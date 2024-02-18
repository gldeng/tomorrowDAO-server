using System.Collections.Generic;
using TomorrowDAOServer.Organization.Dto;
using TomorrowDAOServer.Vote.Dto;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalDetailDto : ProposalListDto
{
    public OrganizationDto OrganizationInfo { get; set; }
    
    public List<VoteRecordDto> VoteTopList { get; set; }
}