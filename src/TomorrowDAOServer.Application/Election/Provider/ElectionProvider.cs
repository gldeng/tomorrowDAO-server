using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.Extensions.Logging;
using TomorrowDAOServer.Common.GraphQL;
using TomorrowDAOServer.Election.Dto;
using TomorrowDAOServer.Election.Index;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.Election.Provider;

public interface IElectionProvider
{
    Task<PagedResultDto<IndexerElection>> GetHighCouncilListAsync(GetHighCouncilListInput input);
}

public class ElectionProvider : IElectionProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;
    private readonly ILogger<ElectionProvider> _logger;

    public ElectionProvider(IGraphQlHelper graphQlHelper, ILogger<ElectionProvider> logger)
    {
        _graphQlHelper = graphQlHelper;
        _logger = logger;
    }
    
    public async Task<PagedResultDto<IndexerElection>> GetHighCouncilListAsync(GetHighCouncilListInput input)
    {
        try
        {
            var graphQlResponse = await _graphQlHelper.QueryAsync<IndexerElectionResult>(new GraphQLRequest
            {
                Query =
                    @"query($sorting:String,$skipCount:Int!,$maxMaxResultCount:Int!,$chainId:String,$DAOId:String,$highCouncilType:String,$termNumber:Long!){
            data:getHighCouncilListAsync(input: {sorting:$sorting,skipCount:$skipCount,maxMaxResultCount:$maxMaxResultCount,chainId:$chainId,DAOId:$DAOId,highCouncilType:$highCouncilType,termNumber:$termNumber})
                dataList{
                    id,chainId,DAOId,termNumber,highCouncilType,address,votesAmount,stakeAmount
                }
                ,totalCount
            }",
                Variables = new
                {
                    input.Sorting,
                    input.SkipCount,
                    input.MaxResultCount,
                    input.ChainId,
                    input.DAOId,
                    input.HighCouncilType,
                    input.TermNumber
                }
            });
            return new PagedResultDto<IndexerElection>
            {
                TotalCount = graphQlResponse.TotalCount,
                Items = graphQlResponse.DataList
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetHighCouncilListAsyncError chainId {chainId} DAOId {DAOId} termNumber{termNumber}", 
                input.ChainId, input.DAOId, input.TermNumber);
        }

        return new PagedResultDto<IndexerElection>(0, new List<IndexerElection>());
    }
}