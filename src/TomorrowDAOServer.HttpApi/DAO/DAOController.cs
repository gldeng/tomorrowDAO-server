using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomorrowDAOServer.DAO;
using TomorrowDAOServer.DAO.Dtos;
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
    public async Task<DAOInfoDto> GetDAOByIdAsync(GetDAOInfoInput input)
    {
        return await _daoAppService.GetDAOByIdAsync(input);
    }
    
    [HttpGet("hc-member-list")]
    public async Task<List<string>> GetMemberListAsync(GetDAOInfoInput input)
    {
        return await _daoAppService.GetMemberListAsync(input);
    }
    
    [HttpGet("hc-candidate-list")]
    public async Task<List<string>> GetCandidateListAsync(GetDAOInfoInput input)
    {
        return await _daoAppService.GetCandidateListAsync(input);
    }
    
    [HttpGet("dao-list")]
    public async Task<GetDAOListResponseDto> GetDAOListAsync(GetDAOListRequestDto request)
    {
        return await _daoAppService.GetDAOListAsync(request);
    }
}