using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomorrowDAOServer.DAO.Dtos;
using TomorrowDAOServer.Common.Provider;
using TomorrowDAOServer.Election.Dto;
using TomorrowDAOServer.Election.Provider;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.DAO.Provider;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;
using AElf.Client;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.Proposal.Provider;

namespace TomorrowDAOServer.DAO;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class DAOAppService : ApplicationService, IDAOAppService
{
    private readonly IDAOProvider _daoProvider;
    private readonly IElectionProvider _electionProvider;
    private readonly IProposalProvider _proposalProvider;
    private readonly IGraphQLProvider _graphQlProvider;
    private readonly IOptionsMonitor<AelfApiInfoOptions> _aelfApiOptions;
    private const int ZeroSkipCount = 0;
    private const int GetHoldersMaxResultCount = 1;
    private const int GetMemberListMaxResultCount = 100;
    private const int CandidateTermNumber = 0;
    
    public DAOAppService(IDAOProvider daoProvider,
        IElectionProvider electionProvider,
        IProposalProvider proposalProvider,
        IGraphQLProvider graphQlProvider,
        IOptionsMonitor<AelfApiInfoOptions> aelfApiOptions)
    {
        _daoProvider = daoProvider;
        _electionProvider = electionProvider;
        _proposalProvider = proposalProvider;
        _graphQlProvider = graphQlProvider;
        _aelfApiOptions = aelfApiOptions;
    }

    public async Task<DAOInfoDto> GetDAOByIdAsync(GetDAOInfoInput input)
    {
        var daoIndex = await _daoProvider.GetAsync(input);
        return ObjectMapper.Map<DAOIndex, DAOInfoDto>(daoIndex);
    }

    public async Task<PagedResultDto<HcMemberDto>> GetMemberListAsync(GetHcMemberInput input)
    {
        var type = input.Type.IsNullOrWhiteSpace() ? HighCouncilType.Member.ToString() : input.Type;
        var daoInfo = await _daoProvider.GetAsync(input);
        var result = await _electionProvider.GetHighCouncilListAsync(new GetHighCouncilListInput
        {
            ChainId = input.ChainId,
            DAOId = input.DAOId,
            HighCouncilType = type,
            TermNumber = type == HighCouncilType.Member.ToString()
                ? daoInfo?.HighCouncilTermNumber ?? 0
                : CandidateTermNumber,
            MaxResultCount = type == HighCouncilType.Member.ToString()
                ? daoInfo?.HighCouncilConfig?.MaxHighCouncilMemberCount ?? GetMemberListMaxResultCount
                : input.MaxResultCount,
            SkipCount = type == HighCouncilType.Member.ToString() ? ZeroSkipCount : input.SkipCount,
            Sorting = input.Sorting
        });
        return new PagedResultDto<HcMemberDto>
        {
            TotalCount = result?.TotalCount ?? 0,
            Items = result?.Items?.Select(x => new HcMemberDto
            {
                Type = type,
                Address = AddressHelper.ToFullAddress(x.ChainId, x.Address),
                VotesAmount = x.VotesAmount.ToString(),
                StakeAmount = x.StakeAmount.ToString()
            }).ToList() ?? new List<HcMemberDto>()
        };
    }

    public async Task<PagedResultDto<DAOListDto>> GetDAOListAsync(QueryDAOListInput input)
    {
        var (item1, daoList) = await _daoProvider.GetDAOListAsync(input);
        var items = ObjectMapper.Map<List<DAOIndex>, List<DAOListDto>>(daoList);
        foreach (var dto in items.Where(x => !x.Symbol.IsNullOrEmpty()).ToList())
        {
            dto.SymbolHoldersNum = await _graphQlProvider.GetHoldersAsync(dto.Symbol.ToUpper(), input.ChainId,
                ZeroSkipCount, GetHoldersMaxResultCount);
            dto.ProposalsNum = await _proposalProvider.GetProposalCountByDAOIds(input.ChainId, dto.DaoId);
        }

        return new PagedResultDto<DAOListDto>
        {
            TotalCount = item1,
            Items = items
        };
    }
}