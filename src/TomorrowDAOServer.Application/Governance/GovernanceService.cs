using System.Collections.Generic;
using System.Threading.Tasks;
using TomorrowDAOServer.Governance.Dto;
using TomorrowDAOServer.Governance.Provider;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Governance;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class GovernanceService : TomorrowDAOServerAppService, IGovernanceService
{
    private readonly IObjectMapper _objectMapper;
    private readonly IGovernanceProvider _governanceProvider;

    public GovernanceService(IGovernanceProvider governanceProvider,
        IObjectMapper objectMapper)
    {
        _governanceProvider = governanceProvider;
        _objectMapper = objectMapper;
    }

    public async Task<GovernanceMechanismDto> GetGovernanceMechanismAsync(string chainId)
    {
        var result = await _governanceProvider.GetGovernanceMechanismAsync(chainId);
        return new GovernanceMechanismDto
        {
            ChainId = chainId,
            GovernanceMechanismList = _objectMapper.Map<List<IndexerGovernanceMechanism>, List<GovernanceMechanismInfo>>(result)
        };
    }
}