using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomorrowDAOServer.Vote;
using TomorrowDAOServer.Vote.Dto;
using Volo.Abp;

namespace TomorrowDAOServer.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("Vote")]
[Route("api/vote")]
public class VoteController
{
    private readonly IVoteService _voteService;

    public VoteController(IVoteService voteService)
    {
        _voteService = voteService;
    }
    
    [HttpGet]
    [Route("/vote-scheme-list")]
    public async Task<VoteSchemeDetailDto> VoteSchemeList(GetVoteSchemeInput input)
    {
        return await _voteService.GetVoteSchemeAsync(input);
    }
}