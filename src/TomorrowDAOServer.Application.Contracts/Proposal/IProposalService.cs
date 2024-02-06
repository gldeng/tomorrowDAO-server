using System.Threading.Tasks;
using TomorrowDAOServer.Proposal.Dto;
using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.Proposal;

public interface IProposalService
{
    Task<PagedResultDto<ProposalDto>> QueryProposalListAsync(QueryProposalListInput input);
}