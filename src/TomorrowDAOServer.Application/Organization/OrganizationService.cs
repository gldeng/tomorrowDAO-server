using System.Collections.Generic;
using System.Threading.Tasks;
using TomorrowDAOServer.DAO;
using TomorrowDAOServer.DAO.Dtos;
using TomorrowDAOServer.Organization.Dto;
using TomorrowDAOServer.Organization.Index;
using TomorrowDAOServer.Organization.Provider;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Organization;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class OrganizationService : TomorrowDAOServerAppService, IOrganizationService
{
    private readonly IObjectMapper _objectMapper;
    private readonly IDAOAppService _daoAppService;
    private readonly IOrganizationInfoProvider _organizationInfoProvider;

    public OrganizationService(IDAOAppService daoAppService,
        IOrganizationInfoProvider organizationInfoProvider,
        IObjectMapper objectMapper)
    {
        _daoAppService = daoAppService;
        _organizationInfoProvider = organizationInfoProvider;
        _objectMapper = objectMapper;
    }

    public async Task<List<OrganizationDto>> GetOrganizationListAsync(GetOrganizationListInput input)
    {
        var DAOInfo = await _daoAppService.GetDAOByIdAsync(new GetDAOInfoInput()
        {
            ChainId = input.ChainId,
            DAOId = input.DAOId
        });

        var result = new List<OrganizationDto>();
        if (DAOInfo == null)
        {
            return result;
        }

        var indexerOrganizationInfos =
            await _organizationInfoProvider.GetOrganizationInfosAsync(input.ChainId, null,
                DAOInfo.GovernanceSchemeId);
        result = _objectMapper.Map<List<IndexerOrganizationInfo>, List<OrganizationDto>>(indexerOrganizationInfos);
        return result;
    }
}