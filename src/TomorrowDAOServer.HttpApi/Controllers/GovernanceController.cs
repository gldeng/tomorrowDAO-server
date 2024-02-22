using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomorrowDAOServer.Contract.Dto;
using TomorrowDAOServer.Governance;
using TomorrowDAOServer.Governance.Dto;
using Volo.Abp;

namespace TomorrowDAOServer.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("Governance")]
[Route("api/")]
public class GovernanceController
{
    private readonly IGovernanceService _governanceService;

    public GovernanceController(IGovernanceService governanceService)
    {
        _governanceService = governanceService;
    }
    
    [HttpGet]
    [Route("governance-model-list")]
    public async Task<GovernanceMechanismDto> GovernanceModelList(QueryFunctionListInput input)
    {
        return await _governanceService.GetGovernanceMechanismAsync(input.ChainId);
    }
}