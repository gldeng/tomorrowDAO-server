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
    Task<Dictionary<string, IndexerOrganizationInfo>> GetOrganizationInfosMemory(string chainId,
        List<string> organizationAddressList);
}

public class OrganizationInfoProvider : IOrganizationInfoProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;

    public OrganizationInfoProvider(IGraphQlHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
    }
    
    public async Task<Dictionary<string, IndexerOrganizationInfo>> GetOrganizationInfosMemory(string chainId, List<string> organizationAddressList)
    {
        if (organizationAddressList.IsNullOrEmpty())
        {
            return new();
        }

        var result = await _graphQlHelper.QueryAsync<IndexerCommonResult<IndexerOrganizationInfos>>(new GraphQLRequest
        {
            Query = @"
			    query($chainId: String!,$organizationAddressList: [organizationAddress!]!) {
                    dataList:getVoteInfosMemory(input:{chainId:$chainId,organizationAddressList:$organizationAddressList}) {
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
}