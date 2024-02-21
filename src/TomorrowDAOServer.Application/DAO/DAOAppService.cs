using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Nest;
using TomorrowDAOServer.DAO.Dtos;
using TomorrowDAOServer.Common.Provider;
using TomorrowDAOServer.Election.Dto;
using TomorrowDAOServer.Election.Provider;
using TomorrowDAOServer.Enums;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;
using AElf.Client;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Options;
using System.Linq;

namespace TomorrowDAOServer.DAO;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class DAOAppService : ApplicationService, IDAOAppService
{
    private readonly INESTRepository<DAOIndex, string> _daoIndexRepository; 
    private readonly IOptionsMonitor<AelfApiInfoOptions> _aelfApiOptions;
    private readonly IGraphQLProvider _graphQlProvider;
    private readonly IElectionProvider _electionProvider;
    private const int ZeroSkipCount = 0;
    private const int GetHoldersMaxResultCount = 1;
    private const int GetMemberListMaxResultCount = 100;
    private const int CandidateTermNumber = 0;

    public DAOAppService(INESTRepository<DAOIndex, string> daoIndexRepository, IGraphQLProvider graphQlProvider,IOptionsMonitor<AelfApiInfoOptions> aelfApiOptions)
    {
        _daoIndexRepository = daoIndexRepository;
        _graphQlProvider = graphQlProvider;
        _aelfApiOptions = aelfApiOptions;
    }

    public async Task<DAOInfoDto> GetDAOByIdAsync(GetDAOInfoInput input)
    {
        return await GetAsync(input);
    }

    public async Task<List<string>> GetMemberListAsync(GetDAOInfoInput input)
    {
        var daoInfo = await GetAsync(input);
        var result = await _electionProvider.GetHighCouncilListAsync(new GetHighCouncilListInput
        {
            ChainId = input.ChainId,
            DAOId = input.DAOId,
            HighCouncilType = HighCouncilType.Member.ToString(),
            TermNumber = daoInfo?.HighCouncilTermNumber ?? 0,
            MaxResultCount = GetMemberListMaxResultCount,
            SkipCount = ZeroSkipCount
        });
        return result?.Items?.Select(x => x.Address).ToList() ?? new List<string>();
    }
    
    public async Task<PagedResultDto<string>> GetCandidateListAsync(GetHcCandidatesInput input)
    {
        var result = await _electionProvider.GetHighCouncilListAsync(new GetHighCouncilListInput
        {
            ChainId = input.ChainId,
            DAOId = input.DAOId,
            HighCouncilType = HighCouncilType.Candidate.ToString(),
            TermNumber = CandidateTermNumber,
            MaxResultCount = input.MaxResultCount,
            SkipCount = input.SkipCount
        });
        return new PagedResultDto<string>
        {
            TotalCount = result?.TotalCount ?? 0,
            Items = result?.Items?.Select(x => x.Address).ToList() ?? new List<string>()
        };
    }

    private async Task<DAOInfoDto> GetAsync(GetDAOInfoInput info)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<DAOIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(t => t.ChainId).Value(info.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(t => t.Id).Value(info.DAOId)));

        QueryContainer Filter(QueryContainerDescriptor<DAOIndex> f) => f.Bool(b => b.Must(mustQuery));
        var dao = await _daoIndexRepository.GetAsync(Filter);
        return ObjectMapper.Map<DAOIndex, DAOInfoDto>(dao);
    }
    
    public async Task<PagedResultDto<DAOListDto>> GetDAOListAsync(QueryDAOListInput request)
    {
        var chainId = request.ChainId;
        var mustQuery = new List<Func<QueryContainerDescriptor<DAOIndex>, QueryContainer>>
        {
            q => 
                q.Term(i => i.Field(t => t.ChainId).Value(chainId))
        };
        QueryContainer Filter(QueryContainerDescriptor<DAOIndex> f) => f.Bool(b => b.Must(mustQuery));
        var (item1, list) = await _daoIndexRepository.GetSortListAsync(Filter, skip: request.SkipCount, limit: request.MaxResultCount, 
            sortFunc: _ => new SortDescriptor<DAOIndex>().Descending(index => index.CreateTime));
        var items = ObjectMapper.Map<List<DAOIndex>, List<DAOListDto>>(list);
        foreach (var dto in items.Where(x => !x.Symbol.IsNullOrEmpty()).ToList())
        {
            dto.SymbolHoldersNum = await _graphQlProvider.GetHoldersAsync(dto.Symbol.ToUpper(), chainId, ZeroSkipCount, GetHoldersMaxResultCount);
        }
        return new PagedResultDto<DAOListDto>
        {
            TotalCount = item1,
            Items = items
        };
    }
    
    public async Task<List<string>> GetContractInfoAsync(string chainId, string contractAddress)
    {
        var client = new AElfClientBuilder().UseEndpoint(BuildDomainUrl(chainId)).Build();
        var bytes = client.GetContractFileDescriptorSetAsync(contractAddress).Result;
        var fileDescriptorSet = FileDescriptorSet.Parser.ParseFrom(bytes);
        var methods =
            (from file in fileDescriptorSet.File
                from service in file.Service
                from method in service.Method
                select method.Name).ToList();

        return methods;
    }
    
    public string BuildDomainUrl(string chainId)
    {
        if (chainId.IsNullOrWhiteSpace())
        {
            return string.Empty;
        }

        var temp = _aelfApiOptions.CurrentValue.AelfApiInfos;

        if (_aelfApiOptions.CurrentValue.AelfApiInfos.TryGetValue(chainId, out var info))
        {
            return info.Domain;
        }

        return string.Empty;
    }
}