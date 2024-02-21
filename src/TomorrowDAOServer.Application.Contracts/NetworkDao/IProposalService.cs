using System.Threading.Tasks;
using TomorrowDAOServer.Dtos.NetworkDao;
using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.NetworkDao;

public interface IProposalService
{
    
    Task<HomePageResponse> GetHomePageAsync(HomePageRequest proposalResult);

    Task<PagedResultDto<ProposalListResponse>> GetProposalList(ProposalListRequest request);

}