using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomorrowDAOServer.Proposal;
using TomorrowDAOServer.Proposal.Dto;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace TomorrowDAOServer.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("Proposal")]
[Route("api/app/proposal")]
public class ProposalController : AbpController
{
    private readonly IProposalService _proposalService;

    public ProposalController(IProposalService proposalService)
    {
        _proposalService = proposalService;
    }
    
    [HttpGet("list")]
    public async Task<ProposalPagedResultDto> QueryProposalListAsync(QueryProposalListInput input)
    {
        return await _proposalService.QueryProposalListAsync(input);
    }
    
    [HttpPost("lista")]
    public async Task<ProposalPagedResultDto> aQueryProposalListAsync(QueryProposalListInput input)
    {
         return await _proposalService.QueryProposalListAsync(input);
    }
    
    [HttpGet]
    [Route("detail")]
    public async Task<ProposalDetailDto> QueryProposalDetailAsync(QueryProposalDetailInput input)
    {
        return await _proposalService.QueryProposalDetailAsync(input);
    }
    
    [HttpGet]
    [Route("my-info")]
    public async Task<MyProposalDto> QueryMyInfoAsync(QueryMyProposalInput input)
    {
        return await _proposalService.QueryMyInfoAsync(input);
    }
    
    [HttpGet]
    [Route("vote-history")]
    public async Task<VoteHistoryDto> QueryVoteHistoryAsync(QueryVoteHistoryInput input)
    {
        return await _proposalService.QueryVoteHistoryAsync(input);
    }
}