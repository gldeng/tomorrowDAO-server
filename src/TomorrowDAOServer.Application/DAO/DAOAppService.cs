using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Nest;
using TomorrowDAOServer.Dtos.DAO;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;

namespace TomorrowDAOServer.DAO;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class DAOAppService : ApplicationService, IDAOAppService
{
    private readonly INESTRepository<DAOIndex, Guid> _daoIndexRepository;

    public DAOAppService(INESTRepository<DAOIndex, Guid> daoIndexRepository)
    {
        _daoIndexRepository = daoIndexRepository;
    }

    public async Task<DAODto> GetDAOByIdAsync(GetDAORequestDto request)
    {
        return await GetAsync(request);
    }

    public async Task<List<string>> GetMemberListAsync(GetDAORequestDto request)
    {
        var daoInfo = await GetAsync(request);
        return daoInfo?.MemberList;
    }
    
    public async Task<List<string>> GetCandidateListAsync(GetDAORequestDto request)
    {
        var daoInfo = await GetAsync(request);
        return daoInfo?.CandidateList;
    }

    private async Task<DAODto> GetAsync(GetDAORequestDto request)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<DAOIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(i => i.Field(t => t.ChainId).Value(request.ChainId)));
        mustQuery.Add(q => q.Term(i => i.Field(t => t.DaoId).Value(request.DAOId)));

        QueryContainer Filter(QueryContainerDescriptor<DAOIndex> f) => f.Bool(b => b.Must(mustQuery));
        var dao = await _daoIndexRepository.GetAsync(Filter);
        return ObjectMapper.Map<DAOIndex, DAODto>(dao);
    }
}