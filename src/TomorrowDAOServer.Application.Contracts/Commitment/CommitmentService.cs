using System.Threading.Tasks;
using TomorrowDAOServer.Commitment.Dto;
using TomorrowDAOServer.Proposal.Dto;
using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.Commitment;

public interface ICommitmentQueryService
{
    Task<PagedResultDto<CommitmentDto>> QueryCommitmentsByProposalId(QueryByProposalIdInput input);
    Task<CommitmentDto> QueryCommitmentByProposalIdAndVoter(QueryByProposalIdAndVoter input);
}