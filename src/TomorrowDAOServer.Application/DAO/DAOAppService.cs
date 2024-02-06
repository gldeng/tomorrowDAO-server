using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Nest;
using TomorrowDAOServer.Dtos.DAO;
using Volo.Abp;
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
    private readonly INESTRepository<DAOIndex, Guid> _daoIndexRepository;
    private readonly IOptionsMonitor<AelfApiInfoOptions> _aelfApiOptions;

    public DAOAppService(INESTRepository<DAOIndex, Guid> daoIndexRepository, IOptionsMonitor<AelfApiInfoOptions> aelfApiOptions)
    {
        _daoIndexRepository = daoIndexRepository;
        _aelfApiOptions = aelfApiOptions;
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