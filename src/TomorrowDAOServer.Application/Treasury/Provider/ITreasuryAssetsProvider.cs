using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.GraphQL;
using TomorrowDAOServer.DAO.Indexer;
using TomorrowDAOServer.Treasury.Dto;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.Treasury.Provider;

public interface ITreasuryAssetsProvider
{
    Task<GetTreasuryFundListResult> GetTreasuryAssetsAsync(GetTreasuryAssetsInput input);
}

public class TreasuryAssetsProvider : ITreasuryAssetsProvider, ISingletonDependency
{
    private readonly ILogger<TreasuryAssetsProvider> _logger;
    private readonly IGraphQlHelper _graphQlHelper;

    public TreasuryAssetsProvider(ILogger<TreasuryAssetsProvider> logger, IGraphQlHelper graphQlHelper)
    {
        _logger = logger;
        _graphQlHelper = graphQlHelper;
    }

    public async Task<GetTreasuryFundListResult> GetTreasuryAssetsAsync(GetTreasuryAssetsInput input)
    {
        var response = await _graphQlHelper.QueryAsync<GetTreasuryFundListResult>(new GraphQLRequest
        {
            Query = @"
			    query($chainId:String!,$skipCount:Int!,$maxResultCount:Int!,$daoId:String!,$symbols: [String]) {
                    getTreasuryFundList(input: {chainId:$chainId,skipCount:$skipCount,maxResultCount:$maxResultCount,daoId:$daoId,symbols:$symbols}){
                        item1,
                        item2 {
                            id,
                            chainId,
                            blockHeight,
                            daoId,
                            treasuryAddress,
                            symbol,
                            availableFunds,
                            lockedFunds
                        }
                    }
                }",
            Variables = new
            {
                chainId = input.ChainId,
                daoId = input.DaoId,
                symbols = input.Symbols,
                skipCount = input.SkipCount,
                maxResultCount = input.MaxResultCount
            }
        });
        return response ?? new GetTreasuryFundListResult();
    }
}