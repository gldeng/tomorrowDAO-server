using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Nest;
using TomorrowDAOServer.DAO.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;

namespace TomorrowDAOServer.DAO;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class DAOAppService : ApplicationService, IDAOAppService
{
    private readonly INESTRepository<DAOIndex, string> _daoIndexRepository;

    public DAOAppService(INESTRepository<DAOIndex, string> daoIndexRepository)
    {
        _daoIndexRepository = daoIndexRepository;
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
}