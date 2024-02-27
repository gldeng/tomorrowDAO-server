using System.Collections.Generic;
using System.Threading.Tasks;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Vote.Dto;
using TomorrowDAOServer.Vote.Index;
using TomorrowDAOServer.Vote.Provider;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Vote;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class VoteService : TomorrowDAOServerAppService, IVoteService
{
    private readonly IObjectMapper _objectMapper;
    private readonly IVoteProvider _voteProvider;

    public VoteService(IVoteProvider voteProvider,
        IObjectMapper objectMapper)
    {
        _voteProvider = voteProvider;
        _objectMapper = objectMapper;
    }

    public async Task<VoteSchemeDetailDto> GetVoteSchemeAsync(GetVoteSchemeInput input)
    {
        var result = await _voteProvider.GetVoteSchemeAsync(input);
        return new VoteSchemeDetailDto
        {
            VoteSchemeList = _objectMapper.Map<List<IndexerVoteSchemeInfo>, List<VoteSchemeInfoDto>>(result)
        };
    }
}