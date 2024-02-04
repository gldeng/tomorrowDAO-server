using System.Threading.Tasks;
using DnsClient.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Dtos.NetworkDao;
using TomorrowDAOServer.NetworkDao;
using Volo.Abp;

namespace TomorrowDAOServer.Controllers;


[RemoteService]
[Area("app")]
[ControllerName("Network dao")]
[Route("api/proposal-home-page")]
public class NetworkDaoController
{

    private ILogger<NetworkDaoController> _logger;
    private readonly IProposalService _proposalService;

    public NetworkDaoController(IProposalService proposalService, ILogger<NetworkDaoController> logger)
    {
        _proposalService = proposalService;
        _logger = logger;
    }


    [HttpGet]
    public async Task<HomePageResponse> ProposalHomePage(HomePageRequest homePageRequest)
    {
        return await _proposalService.GetHomePageAsync(homePageRequest);
    }
    
    
    
}