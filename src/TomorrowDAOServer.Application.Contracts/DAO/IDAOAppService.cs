using System.Collections.Generic;
using System.Threading.Tasks;
using TomorrowDAOServer.DAO.Dtos;

namespace TomorrowDAOServer.DAO;

public interface IDAOAppService
{
    Task<DAOInfoDto> GetDAOByIdAsync(GetDAOInfoInput input);
    Task<List<string>> GetMemberListAsync(GetDAOInfoInput input);
    Task<List<string>> GetCandidateListAsync(GetDAOInfoInput input);
    Task<GetDAOListResponseDto> GetDAOListAsync(GetDAOListRequestDto request);
}