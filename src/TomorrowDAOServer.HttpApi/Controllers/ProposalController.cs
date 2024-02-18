using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomorrowDAOServer.Proposal;
using TomorrowDAOServer.Proposal.Dto;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("Proposal")]
[Route("api/app/proposal")]
public class ProposalController
{
    private readonly IProposalService _proposalService;

    public ProposalController(IProposalService proposalService)
    {
        _proposalService = proposalService;
    }
    
    [HttpGet]
    [Route("query-list")]
    public async Task<PagedResultDto<ProposalListDto>> QueryProposalListAsync(QueryProposalListInput input)
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
}