using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomorrowDAOServer.DAO;
using TomorrowDAOServer.Dtos.DAO;
using Volo.Abp;

namespace TomorrowDAOServer.Controllers;

[RemoteService]
[Area("app")]
[ControllerName("DAO")]
[Route("api/")]
public class DAOController
{
    private readonly IDAOAppService _daoAppService;
    
    public DAOController(IDAOAppService daoAppService)
    {
        _daoAppService = daoAppService;
    }
    
    [HttpGet("dao-info")]
    public async Task<DAODto> GetDAOByIdAsync(GetDAORequestDto request)
    {
        return await _daoAppService.GetDAOByIdAsync(request);
    }
    
    [HttpGet("hc-member-list")]
    public async Task<List<string>> GetMemberListAsync(GetDAORequestDto request)
    {
        return await _daoAppService.GetMemberListAsync(request);
    }
    
    [HttpGet("hc-candidate-list")]
    public async Task<List<string>> GetCandidateListAsync(GetDAORequestDto request)
    {
        return await _daoAppService.GetCandidateListAsync(request);
    }
}