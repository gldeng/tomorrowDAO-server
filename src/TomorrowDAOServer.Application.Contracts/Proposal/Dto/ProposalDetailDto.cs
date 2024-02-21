using System.Collections.Generic;
using TomorrowDAOServer.Organization.Dto;
using TomorrowDAOServer.Vote.Dto;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalDetailDto : ProposalDto
{
    public OrganizationDto OrganizationInfo { get; set; }
    
    public List<VoteRecordDto> VoteTopList { get; set; }
}