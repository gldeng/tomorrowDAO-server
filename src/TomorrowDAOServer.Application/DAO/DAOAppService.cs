using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Nest;
using TomorrowDAOServer.DAO.Dtos;
using TomorrowDAOServer.Common.Provider;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;

namespace TomorrowDAOServer.DAO;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class DAOAppService : ApplicationService, IDAOAppService
{
    private readonly INESTRepository<DAOIndex, string> _daoIndexRepository;
    private readonly IGraphQLProvider _graphQlProvider;
    private const int GetHoldersSkipCount = 0;
    private const int GetHoldersMaxResultCount = 1;

    public DAOAppService(INESTRepository<DAOIndex, string> daoIndexRepository, IGraphQLProvider graphQlProvider)
    {
        _daoIndexRepository = daoIndexRepository;
        _graphQlProvider = graphQlProvider;
    }

    public async Task<DAOInfoDto> GetDAOByIdAsync(GetDAOInfoInput input)
    {
        return await GetAsync(input);
    }

    public async Task<List<string>> GetMemberListAsync(GetDAOInfoInput input)
    {
        var daoInfo = await GetAsync(input);
        return daoInfo?.MemberList;
    }
    
    public async Task<List<string>> GetCandidateListAsync(GetDAOInfoInput input)
    {
        var daoInfo = await GetAsync(input);
        return daoInfo?.CandidateList;
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
            dto.SymbolHoldersNum = await _graphQlProvider.GetHoldersAsync(dto.Symbol.ToUpper(), chainId, GetHoldersSkipCount, GetHoldersMaxResultCount);
        }
        return new PagedResultDto<DAOListDto>
        {
            TotalCount = item1,
            Items = items
        };
    }
}