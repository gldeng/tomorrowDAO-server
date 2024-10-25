using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomorrowDAOServer.Commitment;
using TomorrowDAOServer.Commitment.Dto;
using TomorrowDAOServer.Proposal.Dto;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace TomorrowDAOServer.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("Controller")]
[Route("api/app/commitment")]
public class CommitmentController : AbpController
{
    private readonly ICommitmentQueryService _commitmentQueryService;

    public CommitmentController(ICommitmentQueryService commitmentQueryService)
    {
        _commitmentQueryService = commitmentQueryService;
    }


    [HttpPost("list")]
    public async Task<PagedResultDto<CommitmentDto>> QueryCommitmentByProposalId(QueryByProposalIdInput input)
    {
        return await _commitmentQueryService.QueryCommitmentsByProposalId(input);
    }

    [HttpGet]
    [Route("detail")]
    public async Task<CommitmentDto> QueryProposalDetailAsync(QueryByProposalIdAndVoter input)
    {
        return await _commitmentQueryService.QueryCommitmentByProposalIdAndVoter(input);
    }
}