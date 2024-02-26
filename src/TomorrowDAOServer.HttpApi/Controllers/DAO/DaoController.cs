using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TomorrowDAOServer.DAO;
using Volo.Abp;
using TomorrowDAOServer.Dao.Dto;
using TomorrowDAOServer.DAO.Dtos;
using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.Controllers.Dao;

[RemoteService]
[Area("app")]
[ControllerName("Dao")]
[Route("api/app/dao")]
public class DaoController
{
    private readonly IDAOAppService _daoAppService;

    public DaoController(IDAOAppService daoAppService)
    {
        _daoAppService = daoAppService;
    }
    
    [HttpGet("dao-info")]
    public async Task<DAOInfoDto> GetDAOByIdAsync(GetDAOInfoInput input)
    {
        return await _daoAppService.GetDAOByIdAsync(input);
    }
    
    [HttpGet("hc-member-list")]
    public async Task<PagedResultDto<HcMemberDto>> GetMemberListAsync(GetHcMemberInput input)
    {
        return await _daoAppService.GetMemberListAsync(input);
    }
    
    [HttpGet("dao-list")]
    public async Task<PagedResultDto<DAOListDto>> GetDAOListAsync(QueryDAOListInput request)
    {
        return await _daoAppService.GetDAOListAsync(request);
    }
    
    [HttpGet("contract-info")]
    public async Task<List<string>> GetContractInfoAsync(GetContractInfoInput input)
    {
        return await _daoAppService.GetContractInfoAsync(input.ChainId, input.ContractAddress);
    }
}