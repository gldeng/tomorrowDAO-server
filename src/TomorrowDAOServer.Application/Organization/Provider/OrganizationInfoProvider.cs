using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.GraphQL;
using TomorrowDAOServer.Organization.Index;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.Organization.Provider;

public interface IOrganizationInfoProvider
{
    Task<Dictionary<string, IndexerOrganizationInfo>> GetOrganizationInfosMemoryAsync(string chainId,
        List<string> organizationAddressList);

    Task<List<IndexerOrganizationInfo>> GetOrganizationInfosAsync(string chainId, List<string> organizationAddressList,
        string governanceSchemeId);
}

public class OrganizationInfoProvider : IOrganizationInfoProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;

    public OrganizationInfoProvider(IGraphQlHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
    }
    
    public async Task<Dictionary<string, IndexerOrganizationInfo>> GetOrganizationInfosMemoryAsync(string chainId, List<string> organizationAddressList)
    {
        if (organizationAddressList.IsNullOrEmpty())
        {
            return new();
        }

        var result = await _graphQlHelper.QueryAsync<IndexerCommonResult<IndexerOrganizationInfos>>(new GraphQLRequest
        {
            Query = @"
			    query($chainId: String!,$organizationAddressList: [organizationAddress!]!) {
                    dataList:getOrganizationInfosMemory(input:{chainId:$chainId,organizationAddressList:$organizationAddressList}) {
                        organizationName,
                        organizationAddress,
                        organizationMemberCount
                    }
                  }",
            Variables = new
            {
                chainId, organizationAddressList
            }
        });
        var infos = result.Data?.Data ?? new IndexerOrganizationInfos();
        return infos.DataList.ToDictionary(info => info.OrganizationAddress, info => info);
    }
    
    public async Task<List<IndexerOrganizationInfo>> GetOrganizationInfosAsync(string chainId, List<string> organizationAddressList, string governanceSchemeId)
    {
        var result = await _graphQlHelper.QueryAsync<IndexerCommonResult<IndexerOrganizationInfos>>(new GraphQLRequest
        {
            Query = @"
			    query($chainId: String!,$organizationAddressList: [organizationAddress!],$governanceSchemeId: String) {
                    dataList:getOrganizationInfos(input:{chainId:$chainId,governanceSchemeId:$governanceSchemeId}) {
                        organizationName,
                        organizationAddress,
                        organizationMemberCount
                    }
                  }",
            Variables = new
            {
                chainId, governanceSchemeId
            }
        });
        var infos = result.Data?.Data ?? new IndexerOrganizationInfos();
        return infos.DataList;
    }
}