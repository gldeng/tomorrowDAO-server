using System.Threading.Tasks;
using TomorrowDAOServer.Governance.Dto;

namespace TomorrowDAOServer.Governance;

public interface IGovernanceService
{
    Task<GovernanceMechanismDto> GetGovernanceMechanismAsync(string chainId);
}