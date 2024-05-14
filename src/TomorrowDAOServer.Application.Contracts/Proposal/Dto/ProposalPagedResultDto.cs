using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalPagedResultDto : PagedResultDto<ProposalDto>
{
    public PageInfo PageInfo { get; set; }
}