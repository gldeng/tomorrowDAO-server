using System.Threading.Tasks;
using TomorrowDAOServer.Proposal.Dto;
using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.Proposal;

public interface IProposalService
{
    Task<ProposalPagedResultDto> QueryProposalListAsync(QueryProposalListInput input);
    
    Task<ProposalDetailDto> QueryProposalDetailAsync(QueryProposalDetailInput input);

    Task<MyProposalDto> QueryMyInfoAsync(QueryMyProposalInput input);
    Task<VoteHistoryDto> QueryVoteHistoryAsync(QueryVoteHistoryInput input);
}