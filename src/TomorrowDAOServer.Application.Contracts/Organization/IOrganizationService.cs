using System.Collections.Generic;
using System.Threading.Tasks;
using TomorrowDAOServer.Organization.Dto;

namespace TomorrowDAOServer.Organization;

public interface IOrganizationService
{
    Task<List<OrganizationDto>> GetOrganizationListAsync(GetOrganizationListInput input);
}