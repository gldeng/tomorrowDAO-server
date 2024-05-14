using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.Proposal.Dto;

public class ProposalPagedResultDto : PagedResultDto<ProposalDto>
{
    public PageInfo PreviousPageInfo { get; set; }
    public PageInfo NextPageInfo { get; set; }
}