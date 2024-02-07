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
    [Route("list")]
    public async Task<PagedResultDto<ProposalDto>> GetTokenAsync(QueryProposalListInput input)
    {
         return await _proposalService.QueryProposalListAsync(input);
    }
}