using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using TomorrowDAOServer.Common.GraphQL;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.Governance.Provider;

public interface IGovernanceProvider
{
    Task<List<IndexerGovernanceMechanism>> GetGovernanceMechanismAsync(string chainId);
}

public class GovernanceProvider : IGovernanceProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;

    public GovernanceProvider(IGraphQlHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
    }

    public async Task<List<IndexerGovernanceMechanism>> GetGovernanceMechanismAsync(string chainId)
    {
        var graphQlResponse = await _graphQlHelper.QueryAsync<IndexerGovernanceMechanismResult>(new GraphQLRequest
        {
            Query =
                @"query($chainId:String){
            data:getGovernanceModesAsync(input: {chainId:$chainId})
            {
                id,governanceMechanism
            }}",
            Variables = new
            {
                chainId
            }
        });
        return graphQlResponse?.DataList ?? new List<IndexerGovernanceMechanism>();
    }
}