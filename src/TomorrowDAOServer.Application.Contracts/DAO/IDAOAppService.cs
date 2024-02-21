using System.Collections.Generic;
using System.Threading.Tasks;
using TomorrowDAOServer.DAO.Dtos;
using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.DAO;

public interface IDAOAppService
{
    Task<List<string>> GetContractInfoAsync(string chainId, string address);
    Task<DAOInfoDto> GetDAOByIdAsync(GetDAOInfoInput input);
    Task<List<string>> GetMemberListAsync(GetDAOInfoInput input);
    Task<List<string>> GetCandidateListAsync(GetDAOInfoInput input);
    Task<PagedResultDto<DAOListDto>> GetDAOListAsync(QueryDAOListInput request);
}