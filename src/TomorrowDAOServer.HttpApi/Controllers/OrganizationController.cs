using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomorrowDAOServer.Organization;
using TomorrowDAOServer.Organization.Dto;
using Volo.Abp;

namespace TomorrowDAOServer.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("Organization")]
[Route("api/app/organization")]
public class OrganizationController
{

    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }
    
    [HttpGet]
    [Route("list")]
    public async Task<List<OrganizationDto>> GetOrganizationListAsync(GetOrganizationListInput input)
    {
        return await _organizationService.GetOrganizationListAsync(input);
    }
}