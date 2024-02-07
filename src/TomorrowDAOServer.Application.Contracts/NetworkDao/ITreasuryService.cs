using System.Threading.Tasks;
using TomorrowDAOServer.Dtos.NetworkDao;
using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.NetworkDao;

public interface ITreasuryService
{

    Task<TreasuryBalanceResponse> GetBalanceAsync(TreasuryBalanceRequest request);

    Task<PagedResultDto<TreasuryTransactionDto>> GetTreasuryTransactionAsync(TreasuryTransactionRequest request);
}