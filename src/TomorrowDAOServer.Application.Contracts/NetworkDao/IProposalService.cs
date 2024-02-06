using System.Threading.Tasks;
using TomorrowDAOServer.Dtos.NetworkDao;

namespace TomorrowDAOServer.NetworkDao;

public interface IProposalService
{
    
    Task<HomePageResponse> GetHomePageAsync(HomePageRequest proposalResult);

    
    
}