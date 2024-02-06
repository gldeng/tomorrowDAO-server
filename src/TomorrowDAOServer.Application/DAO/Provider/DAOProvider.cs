using System.Collections.Generic;
using System.Threading.Tasks;
using TomorrowDAOServer.Common.GraphQL;
using GraphQL;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.DAO.Indexer;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.DAO.Provider;

public interface IDAOProvider
{
    Task<List<IndexerDAOInfo>> GetDAOListAsync(GetChainBlockHeightInput input);
}

public class DAOProvider : IDAOProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;

    public DAOProvider(IGraphQlHelper graphQlHelper)
    {
        _graphQlHelper = graphQlHelper;
    }

    public async Task<List<IndexerDAOInfo>> GetDAOListAsync(GetChainBlockHeightInput input)
    {
        var response =  await _graphQlHelper.QueryAsync<IndexerDAOInfos>(new GraphQLRequest
        {
            Query = @"
			    query($chainId:String!,$skipCount:Int!,$maxResultCount:Int!,$startBlockHeight:Long!,$endBlockHeight:Long!) {
                    getDAOList(input: {chainId:$chainId,skipCount:$skipCount,maxResultCount:$maxResultCount,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight}){
                        id,
                        chainId,
                        blockHeight,
                        creator,
                        metadataAdmin,
                        metadata: {
                            name,
                            logoUrl,
                            description,
                            socialMedia
                        },
                        governanceToken,
                        governanceSchemeId,
                        isHighCouncilEnabled,
                        highCouncilConfig: {
                            maxHighCouncilMemberCount,
                            maxHighCouncilCandidateCount,
                            electionPeriod,
                            isRequireHighCouncilForExecution
                        },
                        highCouncilTermNumber,
                        fileInfoList,
                        isTreasuryContractNeeded,
                        isVoteContractNeeded,
                        subsistStatus,
                        treasuryContractAddress,
                        treasuryAccountAddress,
                        isTreasuryPause,
                        treasuryPauseExecutor,
                        voteContractAddress,
                        permissionAddress,
                        permissionInfoList,
                        createTime
                    }
                }",
            Variables = new
            {
                input.ChainId,
                input.SkipCount,
                input.MaxResultCount,
                input.StartBlockHeight,
                input.EndBlockHeight
            }
        });
        return response?.DAOInfos ?? new List<IndexerDAOInfo>();
    }
}