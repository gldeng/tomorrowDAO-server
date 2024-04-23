using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using GraphQL;
using Nest;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.GraphQL;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Proposal.Dto;
using TomorrowDAOServer.Proposal.Index;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.Proposal.Provider;

public interface IProposalProvider
{
    Task<List<IndexerProposal>> GetSyncProposalDataAsync(int skipCount, string chainId, long startBlockHeight,
        long endBlockHeight);

    public Task<Tuple<long, List<ProposalIndex>>> GetProposalListAsync(QueryProposalListInput input);
    
    public Task<ProposalIndex> GetProposalByIdAsync(string chainId, string proposalId);
    
    public Task<Dictionary<string, ProposalIndex>> GetProposalListByIds(string chainId, List<string> ids);

    public Task<long> GetProposalCountByDAOIds(string chainId, string DAOId);

    public Task BulkAddOrUpdateAsync(List<ProposalIndex> list);

    public Task<List<ProposalIndex>> GetExpiredProposalListAsync(int skipCount, List<ProposalStatus> statusList);
}

public class ProposalProvider : IProposalProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;
    private readonly INESTRepository<ProposalIndex, string> _proposalIndexRepository;

    public ProposalProvider(IGraphQlHelper graphQlHelper,
        INESTRepository<ProposalIndex, string> proposalIndexRepository)
    {
        _graphQlHelper = graphQlHelper;
        _proposalIndexRepository = proposalIndexRepository;
    }

    public async Task<List<IndexerProposal>> GetSyncProposalDataAsync(int skipCount, string chainId,
        long startBlockHeight, long endBlockHeight)
    {
        var graphQlResponse = await _graphQlHelper.QueryAsync<IndexerProposalSync>(new GraphQLRequest
        {
            Query =
                @"query($skipCount:Int!,$chainId:String,$startBlockHeight:Long!,$endBlockHeight:Long!){
            dataList:getSyncProposalInfos(input: {skipCount:$skipCount,chainId:$chainId,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight})
            {
                id,chainId,blockHeight,blockHash
                DAOId,proposalId,proposalTitle,proposalType,governanceType,proposalStatus,startTime,endTime,expiredTime,
                organizationAddress,executeAddress,proposalDescription,transaction{toAddress,contractMethodName,params{key,value}},
                governanceSchemeId,voteSchemeId,executeByHighCouncil,deployTime,executeTime,
                minimalRequiredThreshold,minimalVoteThreshold,minimalApproveThreshold,maximalRejectionThreshold,maximalAbstentionThreshold
            }}",
            Variables = new
            {
                skipCount,
                chainId,
                startBlockHeight,
                endBlockHeight
            }
        });
        return graphQlResponse?.DataList ?? new List<IndexerProposal>();
    }

    public async Task<Tuple<long, List<ProposalIndex>>> GetProposalListAsync(QueryProposalListInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<ProposalIndex>, QueryContainer>>();

        var shouldQuery = new List<Func<QueryContainerDescriptor<ProposalIndex>, QueryContainer>>();

        AssemblyBaseQuery(input, mustQuery);

        AssemblyContentQuery(input.Content, shouldQuery);

        if (shouldQuery.Any())
        {
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        QueryContainer Filter(QueryContainerDescriptor<ProposalIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        //add sorting
        var sortDescriptor = GetQuerySortDescriptor();

        return await _proposalIndexRepository.GetSortListAsync(Filter, sortFunc: sortDescriptor,
            skip: input.SkipCount,
            limit: input.MaxResultCount);
    }

    public async Task<ProposalIndex> GetProposalByIdAsync(string chainId, string proposalId)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<ProposalIndex>, QueryContainer>>();
        
        mustQuery.Add(q => q.Term(i =>
            i.Field(f => f.ChainId).Value(chainId)));
        
        mustQuery.Add(q => q.Term(i =>
            i.Field(f => f.ProposalId).Value(proposalId)));
      
        QueryContainer Filter(QueryContainerDescriptor<ProposalIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        return await _proposalIndexRepository.GetAsync(Filter);
    }

    public async Task<Dictionary<string, ProposalIndex>> GetProposalListByIds(string chainId, List<string> proposalIds)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<ProposalIndex>, QueryContainer>>();
        
        mustQuery.Add(q => q.Term(i =>
            i.Field(f => f.ChainId).Value(chainId)));
        
        mustQuery.Add(q => q.Terms(i =>
            i.Field(f => f.ProposalId).Terms(proposalIds)));
        
        QueryContainer Filter(QueryContainerDescriptor<ProposalIndex> f) =>
            f.Bool(b => b.Must(mustQuery));
        
        var tuple = await _proposalIndexRepository.GetListAsync(Filter);

        return tuple.Item2.ToDictionary(p => p.ProposalId, p => p);
    }

    public async Task<long> GetProposalCountByDAOIds(string chainId, string DAOId)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<ProposalIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Terms(i =>
            i.Field(f => f.ChainId).Terms(chainId)));
        mustQuery.Add(q => q.Terms(i =>
            i.Field(f => f.DAOId).Terms(DAOId)));
        
        QueryContainer Filter(QueryContainerDescriptor<ProposalIndex> f) =>
            f.Bool(b => b.Must(mustQuery));
        
        return (await _proposalIndexRepository.CountAsync(Filter)).Count;
    }

    public async Task BulkAddOrUpdateAsync(List<ProposalIndex> list)
    {
       await _proposalIndexRepository.BulkAddOrUpdateAsync(list);
    }

    public async Task<List<ProposalIndex>> GetExpiredProposalListAsync(int skipCount, List<ProposalStatus> statusList)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<ProposalIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Terms(i =>
            i.Field(f => f.ProposalStatus).Terms(statusList)));

        mustQuery.Add(q => q.TermRange(i
            => i.Field(index => index.ExecuteEndTime.ToUtcMilliSeconds())
                .LessThanOrEquals(DateTime.UtcNow.ToString("O"))));

        QueryContainer Filter(QueryContainerDescriptor<ProposalIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var tuple = await _proposalIndexRepository.GetListAsync(Filter, skip: skipCount);
        return tuple.Item2;
    }
    
    private static void AssemblyBaseQuery(QueryProposalListInput input,
        List<Func<QueryContainerDescriptor<ProposalIndex>, QueryContainer>> mustQuery)
    {
        if (!input.ChainId.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.ChainId).Value(input.ChainId)));
        }
        
        if (!input.DaoId.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.DAOId).Value(input.DaoId)));
        }

        if (input.GovernanceMechanism != null)
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.GovernanceMechanism).Value(input.GovernanceMechanism)));
        }
        
        if (input.ProposalType != null)
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.ProposalType).Value(input.ProposalType)));
        }
        
        if (input.ProposalStatus != null)
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.ProposalStatus).Value(input.ProposalStatus)));
        }
    }

    private static void AssemblyContentQuery(string content,
        List<Func<QueryContainerDescriptor<ProposalIndex>, QueryContainer>> shouldQuery)
    {
        var titleMustQuery = new List<Func<QueryContainerDescriptor<ProposalIndex>, QueryContainer>>();
        var descriptionMustQuery = new List<Func<QueryContainerDescriptor<ProposalIndex>, QueryContainer>>();
        var proposalIdMustQuery = new List<Func<QueryContainerDescriptor<ProposalIndex>, QueryContainer>>();
        
        titleMustQuery.Add(q => q.
            Match(m => m.Field(f => f.ProposalTitle).Query(content)));
        descriptionMustQuery.Add(q => q.
            Match(m => m.Field(f => f.ProposalDescription).Query(content)));
        proposalIdMustQuery.Add(q => q.
            Match(m => m.Field(f => f.ProposalId).Query(content)));
        
        shouldQuery.Add(s => s.Bool(sb => sb.Must(titleMustQuery)));
        shouldQuery.Add(s => s.Bool(sb => sb.Must(descriptionMustQuery)));
        shouldQuery.Add(s => s.Bool(sb => sb.Must(proposalIdMustQuery)));
    }

    private static Func<SortDescriptor<ProposalIndex>, IPromise<IList<ISort>>> GetQuerySortDescriptor()
    {
        //use default
        var sortDescriptor = new SortDescriptor<ProposalIndex>();

        sortDescriptor.Descending(a => a.DeployTime);

        return _ => sortDescriptor;
    }
}