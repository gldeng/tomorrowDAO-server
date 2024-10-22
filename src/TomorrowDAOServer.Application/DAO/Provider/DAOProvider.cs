using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AElf;
using AElf.Indexing.Elasticsearch;
using TomorrowDAOServer.Common.GraphQL;
using GraphQL;
using Microsoft.Extensions.Logging;
using Nest;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.Dtos;
using TomorrowDAOServer.DAO.Dtos;
using TomorrowDAOServer.DAO.Indexer;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.DAO.Provider;

public interface IDAOProvider
{
    Task<List<IndexerDAOInfo>> GetSyncDAOListAsync(GetChainBlockHeightInput input);

    Task<DAOIndex> GetAsync(GetDAOInfoInput input);

    Task<Tuple<long, List<DAOIndex>>> GetDAOListAsync(QueryPageInput input, ISet<string> excludeNames);
    Task<long> GetDAOListCountAsync(QueryPageInput input, ISet<string> excludeNames);
    Task<Tuple<long, List<DAOIndex>>> GetDAOListByNameAsync(string chainId, List<string> names);
    Task<Tuple<long, List<DAOIndex>>> GetMyOwneredDAOListAsync(QueryMyDAOListInput input, string address);
    Task<Tuple<long, List<DAOIndex>>> GetManagedDAOAsync(QueryMyDAOListInput input, List<string> daoIds, bool networkDao);
    Task<PageResultDto<IndexerDAOInfo>> GetMyParticipatedDaoListAsync(GetParticipatedInput input);
    Task<PageResultDto<MemberDto>> GetMemberListAsync(GetMemberListInput listInput);
    Task<MemberDto> GetMemberAsync(GetMemberInput listInput);
    Task<List<DAOIndex>> GetDaoListByDaoIds(string chainId, List<string> daoIds);
}

public class DAOProvider : IDAOProvider, ISingletonDependency
{
    private readonly IGraphQlHelper _graphQlHelper;
    private readonly INESTRepository<DAOIndex, string> _daoIndexRepository;
    private readonly ILogger<DAOProvider> _logger;

    public DAOProvider(IGraphQlHelper graphQlHelper, ILogger<DAOProvider> logger,
        INESTRepository<DAOIndex, string> daoIndexRepository)
    {
        _graphQlHelper = graphQlHelper;
        _daoIndexRepository = new Wrapped<DAOIndex, string>(daoIndexRepository);
        _logger = logger;
    }

    public async Task<List<IndexerDAOInfo>> GetSyncDAOListAsync(GetChainBlockHeightInput input)
    {
        var response = await _graphQlHelper.QueryAsync<IndexerDAOInfos>(new GraphQLRequest
        {
            Query = @"
			    query($chainId:String!,$skipCount:Int!,$maxResultCount:Int!,$startBlockHeight:Long!,$endBlockHeight:Long!) {
                    dAOInfos:getDAOList(input: {chainId:$chainId,skipCount:$skipCount,maxResultCount:$maxResultCount,startBlockHeight:$startBlockHeight,endBlockHeight:$endBlockHeight}){
                        id,
                        chainId,
                        blockHeight,
                        creator,
                        metadata {
                            name,
                            logoUrl,
                            description,
                            socialMedia
                        },
                        governanceToken,
                        isHighCouncilEnabled,
                        highCouncilAddress,
                        maxHighCouncilMemberCount,
                        maxHighCouncilCandidateCount,
                        electionPeriod,
                        stakingAmount,
                        highCouncilTermNumber,
                        fileInfoList,
                        isTreasuryContractNeeded,
                        subsistStatus,
                        treasuryContractAddress,
                        treasuryAccountAddress,
                        isTreasuryPause,
                        treasuryPauseExecutor,
                        voteContractAddress,
                        electionContractAddress,
                        governanceContractAddress,
                        timelockContractAddress,
                        activeTimePeriod,
                        vetoActiveTimePeriod,
                        pendingTimePeriod,
                        executeTimePeriod,
                        vetoExecuteTimePeriod,
                        createTime,
                        isNetworkDAO,
                        voterCount,
                        governanceMechanism
                    }
                }",
            Variables = new
            {
                chainId = input.ChainId,
                skipCount = input.SkipCount,
                maxResultCount = input.MaxResultCount,
                startBlockHeight = input.StartBlockHeight,
                endBlockHeight = input.EndBlockHeight
            }
        });
        return response?.DAOInfos ?? new List<IndexerDAOInfo>();
    }

    public async Task<DAOIndex> GetAsync(GetDAOInfoInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<DAOIndex>, QueryContainer>>();
        if (!input.ChainId.IsNullOrWhiteSpace())
        {
            mustQuery.Add(q => q.Term(i => i.Field(t => t.ChainId).Value(input.ChainId)));
        }
        if (!input.DAOId.IsNullOrWhiteSpace())
        {
            mustQuery.Add(q => q.Term(i => i.Field(t => t.Id).Value(input.DAOId)));
        }
        else
        {
            //input.Alias = Regex.Replace(input.Alias, @"([+\-&|!(){}[\]^""~*?:\\])", @"\$1");
            var aliasHexString = Encoding.UTF8.GetBytes(input.Alias).ToHex();
            mustQuery.Add(q => q.Term(i => i.Field(t => t.AliasHexString).Value(aliasHexString)));
        }

        QueryContainer Filter(QueryContainerDescriptor<DAOIndex> f) => f.Bool(b => b.Must(mustQuery));
        return await _daoIndexRepository.GetAsync(Filter);
    }

    public async Task<Tuple<long, List<DAOIndex>>> GetDAOListAsync(QueryPageInput input, ISet<string> excludeNames)
    {
        var chainId = input.ChainId;
        var mustQuery = new List<Func<QueryContainerDescriptor<DAOIndex>, QueryContainer>>
        {
            q =>
                q.Term(i => i.Field(t => t.ChainId).Value(chainId))
        };
        if (!excludeNames.IsNullOrEmpty())
        {
            mustQuery.Add(q =>
                !q.Terms(i => i.Field(t => t.Metadata.Name).Terms(excludeNames)));
        }

        QueryContainer Filter(QueryContainerDescriptor<DAOIndex> f) => f.Bool(b => b.Must(mustQuery));
        return await _daoIndexRepository.GetSortListAsync(Filter, skip: input.SkipCount, limit: input.MaxResultCount,
            sortFunc: _ => new SortDescriptor<DAOIndex>().Descending(index => index.CreateTime));
    }

    public async Task<long> GetDAOListCountAsync(QueryPageInput input, ISet<string> excludeNames)
    {
        var chainId = input.ChainId;
        var mustQuery = new List<Func<QueryContainerDescriptor<DAOIndex>, QueryContainer>>
        {
            q =>
                q.Term(i => i.Field(t => t.ChainId).Value(chainId))
        };
        if (!excludeNames.IsNullOrEmpty())
        {
            mustQuery.Add(q =>
                !q.Terms(i => i.Field(t => t.Metadata.Name).Terms(excludeNames)));
        }

        QueryContainer Filter(QueryContainerDescriptor<DAOIndex> f) => f.Bool(b => b.Must(mustQuery));
        return (await _daoIndexRepository.CountAsync(Filter)).Count;
    }

    public async Task<Tuple<long, List<DAOIndex>>> GetDAOListByNameAsync(string chainId, List<string> names)
    {
        if (names.IsNullOrEmpty())
        {
            return new Tuple<long, List<DAOIndex>>(0, new List<DAOIndex>());
        }

        var mustQuery = new List<Func<QueryContainerDescriptor<DAOIndex>, QueryContainer>>
        {
            q =>
                q.Term(i => i.Field(t => t.ChainId).Value(chainId)),
            q =>
                q.Terms(i => i.Field(t => t.Metadata.Name).Terms(names))
        };
        QueryContainer Filter(QueryContainerDescriptor<DAOIndex> f) => f.Bool(b => b.Must(mustQuery));
        return await _daoIndexRepository.GetSortListAsync(Filter);
    }

    public async Task<Tuple<long, List<DAOIndex>>> GetMyOwneredDAOListAsync(QueryMyDAOListInput input, string address)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<DAOIndex>, QueryContainer>>
        {
            q =>
                q.Term(i => i.Field(t => t.ChainId).Value(input.ChainId)),
            q =>
                q.Term(i => i.Field(t => t.Creator).Value(address))
        };
        QueryContainer Filter(QueryContainerDescriptor<DAOIndex> f) => f.Bool(b => b.Must(mustQuery));
        return await _daoIndexRepository.GetSortListAsync(Filter, skip: input.SkipCount, limit: input.MaxResultCount,
            sortFunc: _ => new SortDescriptor<DAOIndex>().Descending(index => index.CreateTime));
    }

    public async Task<Tuple<long, List<DAOIndex>>> GetManagedDAOAsync(QueryMyDAOListInput input, List<string> daoIds,
        bool networkDao)
    {
        if (!networkDao && daoIds.IsNullOrEmpty())
        {
            return new Tuple<long, List<DAOIndex>>(0, new List<DAOIndex>());
        }

        var mustQuery = new List<Func<QueryContainerDescriptor<DAOIndex>, QueryContainer>>
        {
            q =>
                q.Term(i => i.Field(t => t.ChainId).Value(input.ChainId))
        };

        var shouldQuery = new List<Func<QueryContainerDescriptor<DAOIndex>, QueryContainer>>();
        if (!daoIds.IsNullOrEmpty())
        {
            shouldQuery.Add(q =>
                q.Terms(i => i.Field(t => t.Id).Terms(daoIds)));
        }

        if (networkDao)
        {
            shouldQuery.Add(q =>
                q.Term(i => i.Field(t => t.IsNetworkDAO).Value(true)));
        }

        QueryContainer Filter(QueryContainerDescriptor<DAOIndex> f) =>
            f.Bool(b => b.Should(shouldQuery).MinimumShouldMatch(1).Must(mustQuery));

        return await _daoIndexRepository.GetSortListAsync(Filter, skip: input.SkipCount, limit: input.MaxResultCount,
            sortFunc: _ => new SortDescriptor<DAOIndex>().Descending(index => index.CreateTime));
    }

    public async Task<PageResultDto<IndexerDAOInfo>> GetMyParticipatedDaoListAsync(GetParticipatedInput input)
    {
        try
        {
            var response = await _graphQlHelper.QueryAsync<IndexerCommonResult<PageResultDto<IndexerDAOInfo>>>(
                new GraphQLRequest
                {
                    Query = @"
			        query($chainId:String!,$skipCount:Int!,$maxResultCount:Int!,$address:String!) {
                        data:getMyParticipated(input: {chainId:$chainId,skipCount:$skipCount,maxResultCount:$maxResultCount,address:$address})
                        {
                            totalCount,
                            data{
                                id,
                                chainId,
                                blockHeight,
                                creator,
                                metadata {
                                    name,
                                    logoUrl,
                                    description,
                                    socialMedia
                                },
                                governanceToken,
                                isHighCouncilEnabled,
                                highCouncilAddress,
                                maxHighCouncilMemberCount,
                                maxHighCouncilCandidateCount,
                                electionPeriod,
                                stakingAmount,
                                highCouncilTermNumber,
                                fileInfoList,
                                isTreasuryContractNeeded,
                                subsistStatus,
                                treasuryContractAddress,
                                treasuryAccountAddress,
                                isTreasuryPause,
                                treasuryPauseExecutor,
                                voteContractAddress,
                                electionContractAddress,
                                governanceContractAddress,
                                timelockContractAddress,
                                activeTimePeriod,
                                vetoActiveTimePeriod,
                                pendingTimePeriod,
                                executeTimePeriod,
                                vetoExecuteTimePeriod,
                                createTime,
                                isNetworkDAO,
                                voterCount
                            }
                        }
                    }",
                    Variables = new
                    {
                        chainId = input.ChainId,
                        skipCount = input.SkipCount,
                        maxResultCount = input.MaxResultCount,
                        address = input.Address
                    }
                });
            return response?.Data ?? new PageResultDto<IndexerDAOInfo>();
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "GetMyParticipatedDaoListAsyncException chainId {chainId}, address {address}, skipCount {skipCount}, maxResultCount {maxResultCount}",
                input.ChainId, input.Address, input.SkipCount, input.MaxResultCount);
            return new PageResultDto<IndexerDAOInfo>();
        }
    }

    public async Task<PageResultDto<MemberDto>> GetMemberListAsync(GetMemberListInput listInput)
    {
        try
        {
            var response = await _graphQlHelper.QueryAsync<IndexerCommonResult<PageResultDto<MemberDto>>>(
                new GraphQLRequest
                {
                    Query = @"
			        query($chainId:String!,$skipCount:Int!,$maxResultCount:Int!,$dAOId:String!) {
                        data:getMemberList(input: {chainId:$chainId,skipCount:$skipCount,maxResultCount:$maxResultCount,dAOId:$dAOId})
                        {
                            totalCount,
                            data{
                                id,
                                chainId,
                                blockHeight,
                                dAOId,    
                                address,
                                createTime
                            }
                        }
                    }",
                    Variables = new
                    {
                        chainId = listInput.ChainId,
                        skipCount = listInput.SkipCount,
                        maxResultCount = listInput.MaxResultCount,
                        dAOId = listInput.DAOId
                    }
                });
            return response?.Data ?? new PageResultDto<MemberDto>();
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "GetMemberListAsync chainId {chainId}, daoId {daoId}, skipCount {skipCount}, maxResultCount {maxResultCount}",
                listInput.ChainId, listInput.DAOId, listInput.SkipCount, listInput.MaxResultCount);
            return new PageResultDto<MemberDto>();
        }
    }

    public async Task<MemberDto> GetMemberAsync(GetMemberInput input)
    {
        try
        {
            var response = await _graphQlHelper.QueryAsync<IndexerCommonResult<MemberDto>>(new GraphQLRequest
            {
                Query = @"
			        query($chainId:String!,$dAOId:String!,$address:String!) {
                        data:getMember(input: {chainId:$chainId,dAOId:$dAOId,address:$address})
                        {
                            id,
                            chainId,
                            blockHeight,
                            dAOId,    
                            address,
                            createTime
                        }
                    }",
                Variables = new
                {
                    chainId = input.ChainId,
                    dAOId = input.DAOId,
                    address = input.Address
                }
            });
            return response?.Data ?? new MemberDto();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetIsMemberAsync chainId {chainId}, daoId {daoId}, address {address}",
                input.ChainId, input.DAOId, input.Address);
            return new MemberDto();
        }
    }

    public async Task<List<DAOIndex>> GetDaoListByDaoIds(string chainId, List<string> daoIds)
    {
        if (daoIds.IsNullOrEmpty())
        {
            return new List<DAOIndex>();
        }

        var mustQuery = new List<Func<QueryContainerDescriptor<DAOIndex>, QueryContainer>>
        {
            q =>
                q.Term(i => i.Field(t => t.ChainId).Value(chainId)),
            q =>
                q.Terms(i => i.Field(t => t.Id).Terms(daoIds))
        };
        QueryContainer Filter(QueryContainerDescriptor<DAOIndex> f) => f.Bool(b => b.Must(mustQuery));
        var result = await _daoIndexRepository.GetListAsync(Filter);
        return result.Item2 ?? new List<DAOIndex>();
    }
}