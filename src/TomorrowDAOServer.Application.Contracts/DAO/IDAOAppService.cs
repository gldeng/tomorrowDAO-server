using System.Collections.Generic;
using System.Threading.Tasks;
using TomorrowDAOServer.Dtos.DAO;

namespace TomorrowDAOServer.DAO;

public interface IDAOAppService
{
    Task<DAODto> GetDAOByIdAsync(GetDAORequestDto request);
    Task<List<string>> GetMemberListAsync(GetDAORequestDto request);
    Task<List<string>> GetCandidateListAsync(GetDAORequestDto request);
    Task<List<string>> GetContractInfoAsync(string chainId, string address);
}