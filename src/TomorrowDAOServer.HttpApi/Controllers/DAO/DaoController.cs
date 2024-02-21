using System.Collections.Generic;
using System.Threading.Tasks;
using TomorrowDAOServer.Dao;
using Microsoft.AspNetCore.Mvc;
using TomorrowDAOServer.DAO;
using Volo.Abp;
using TomorrowDAOServer.Dao.Dto;

namespace TomorrowDAOServer.Controllers.Dao;


[RemoteService]
[Area("app")]
[ControllerName("Dao")]
[Route("api/app/dao")]
public class DaoController
{
    private readonly IDAOAppService _daoService;

    public DaoController(IDAOAppService daoService)
    {
        _daoService = daoService;
    }

    [HttpGet]
    [Route("contract-info")]
    public async Task<List<string>> GetContractInfoAsync(GetContractInfoInput input)
    {
        return await _daoService.GetContractInfoAsync(input.ChainId, input.ContractAddress);
    }
}