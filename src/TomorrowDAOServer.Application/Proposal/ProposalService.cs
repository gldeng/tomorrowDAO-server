using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.DAO.Dtos;
using TomorrowDAOServer.DAO.Provider;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.Proposal.Dto;
using TomorrowDAOServer.Proposal.Provider;
using TomorrowDAOServer.Vote.Dto;
using TomorrowDAOServer.Vote.Index;
using TomorrowDAOServer.Vote.Provider;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;
using Newtonsoft.Json;
using Nito.AsyncEx;
using TomorrowDAOServer.Common.Enum;
using TomorrowDAOServer.Dtos.Explorer;
using TomorrowDAOServer.Providers;
using ProposalType = TomorrowDAOServer.Enums.ProposalType;

namespace TomorrowDAOServer.Proposal;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class ProposalService : TomorrowDAOServerAppService, IProposalService
{
    private const string VoteTopSorting = "Amount DESC";
    private const string DecimalString = "8";
    private const string Symbol = "ELF";
    private readonly IObjectMapper _objectMapper;
    private readonly IOptionsMonitor<ProposalTagOptions> _proposalTagOptionsMonitor;
    private readonly IProposalProvider _proposalProvider;
    private readonly IVoteProvider _voteProvider;
    private readonly IDAOProvider _DAOProvider;
    private readonly IProposalAssistService _proposalAssistService;
    private readonly ILogger<ProposalProvider> _logger;
    private readonly IExplorerProvider _explorerProvider;

    public ProposalService(IObjectMapper objectMapper, IProposalProvider proposalProvider, IVoteProvider voteProvider,
        IExplorerProvider explorerProvider,
        IProposalAssistService proposalAssistService,
        IDAOProvider DAOProvider, IOptionsMonitor<ProposalTagOptions> proposalTagOptionsMonitor,
        ILogger<ProposalProvider> logger)
    {
        _objectMapper = objectMapper;
        _proposalProvider = proposalProvider;
        _voteProvider = voteProvider;
        _proposalTagOptionsMonitor = proposalTagOptionsMonitor;
        _logger = logger;
        _DAOProvider = DAOProvider;
        _proposalAssistService = proposalAssistService;
        _explorerProvider = explorerProvider;
    }

    public async Task<ProposalPagedResultDto> QueryProposalListAsync(QueryProposalListInput input)
    {
        var proposalList = await GetProposalListFromMultiSourceAsync(input);
        if (proposalList.IsNullOrEmpty())
        {
            return new ProposalPagedResultDto();
        }

        //query proposal vote infos
        var proposalIds = proposalList.FindAll(item => item.ProposalSource == ProposalSourceEnum.TMRWDAO)
            .Select(item => item.ProposalId).ToList();
        var voteItemsMap = await _voteProvider.GetVoteItemsAsync(input.ChainId, proposalIds);
        var daoIndex = await _DAOProvider.GetAsync(new GetDAOInfoInput
        {
            ChainId = input.ChainId,
            DAOId = input.DaoId
        });
        var symbol = daoIndex?.GovernanceToken ?? string.Empty;
        var symbolDecimal = await _explorerProvider.GetTokenDecimalAsync(input.ChainId, symbol);
        var voteSchemeDic =
            await _voteProvider.GetVoteSchemeDicAsync(new GetVoteSchemeInput { ChainId = input.ChainId });
        foreach (var proposal in proposalList.Where(proposal => proposal.ProposalSource == ProposalSourceEnum.TMRWDAO))
        {
            if (voteItemsMap.TryGetValue(proposal.ProposalId, out var voteInfo))
            {
                _objectMapper.Map(voteInfo, proposal);
            }

            if (proposal.ProposalType == ProposalType.Advisory.ToString())
            {
                proposal.ExecuteStartTime = null;
                proposal.ExecuteEndTime = null;
                proposal.ExecuteTime = null;
            }

            if (proposal.ProposalStatus != ProposalStatus.Executed.ToString())
            {
                proposal.ExecuteTime = null;
            }

            proposal.VoteMechanismName = voteSchemeDic.TryGetValue(proposal.VoteSchemeId, out var voteMechanism)
                ? voteMechanism.ToString()
                : string.Empty;
            proposal.Symbol = symbol;
            proposal.Decimals = symbolDecimal;
        }

        return new ProposalPagedResultDto
        {
            Items = proposalList,
            TotalCount = proposalList.Count,
            PageInfo = CalcNewPageInfo(proposalList, input.PageInfo)
        };
    }

    private async Task<List<ProposalDto>> GetProposalListFromMultiSourceAsync(QueryProposalListInput input)
    {
        if (!input.IsNetworkDao || (input.ProposalType != null && input.ProposalType != ProposalType.ONCHAIN))
        {
            return await GetProposalListAsync(input, ProposalSourceEnum.TMRWDAO);
        }

        var tasks = new List<Task<List<ProposalDto>>>();
        tasks.Add(GetProposalListAsync(input, ProposalSourceEnum.TMRWDAO));
        tasks.Add(GetProposalListAsync(input, ProposalSourceEnum.ONCHAIN_PARLIAMENT));
        tasks.Add(GetProposalListAsync(input, ProposalSourceEnum.ONCHAIN_REFERENDUM));
        tasks.Add(GetProposalListAsync(input, ProposalSourceEnum.ONCHAIN_ASSOCIATION));

        await tasks.WhenAll();
        var proposalDtoList = new List<ProposalDto>();
        foreach (var task in tasks.Where(task => !task.Result.IsNullOrEmpty()))
        {
            proposalDtoList.AddRange(task.Result);
        }

        return proposalDtoList.OrderByDescending(item => item.DeployTime).Take(input.MaxResultCount).ToList();
    }

    private static PageInfo CalcNewPageInfo(List<ProposalDto> proposalList, [CanBeNull] PageInfo pageInfo)
    {
        var newPageInfo = new PageInfo
        {
            ProposalSkipCount = pageInfo?.ProposalSkipCount ?? new Dictionary<ProposalSourceEnum, int>()
        };
        var skipCount = newPageInfo.ProposalSkipCount;
        foreach (var proposalDto in proposalList)
        {
            var count = skipCount.GetOrDefault(proposalDto.ProposalSource);
            count = count == 0 ? 1 : count;
            skipCount[proposalDto.ProposalSource] = count + 1;
        }

        return newPageInfo;
    }

    private async Task<List<ProposalDto>> GetProposalListAsync(QueryProposalListInput input,
        ProposalSourceEnum proposalSource)
    {
        try
        {
            if (proposalSource == ProposalSourceEnum.TMRWDAO)
            {
                var proposalIndexList = await _proposalProvider.GetProposalListAsync(input);
                return _objectMapper.Map<List<ProposalIndex>, List<ProposalDto>>(proposalIndexList);
            }

            var pageSize = 1;
            if (input.PageInfo != null && !input.PageInfo.ProposalSkipCount.IsNullOrEmpty() &&
                input.PageInfo.ProposalSkipCount.ContainsKey(proposalSource))
            {
                pageSize = input.PageInfo.ProposalSkipCount[proposalSource];
            }

            var proposalType = _explorerProvider.GetProposalType(proposalSource);
            var proposalStatus = _explorerProvider.GetProposalStatus(input.ProposalStatus, null);
            var explorerrResponse = await _explorerProvider.GetProposalPagerAsync(input.ChainId,
                new ExplorerProposalListRequest
                {
                    PageSize = input.MaxResultCount,
                    PageNum = pageSize,
                    ProposalType = proposalType.ToString(),
                    Status = proposalStatus,
                    IsContract = 0,
                    Address = null,
                    Search = input.Content
                });
            return ConvertToProposal(explorerrResponse.List, input, proposalSource);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetProposalList from Source:{source} error, input={daoId}", proposalSource,
                JsonConvert.SerializeObject(input));
            return new List<ProposalDto>();
        }
    }

    private static List<ProposalDto> ConvertToProposal(List<ExplorerProposalResult> explorerProposalList,
        QueryProposalListInput input, ProposalSourceEnum proposalSource)
    {
        if (explorerProposalList.IsNullOrEmpty())
        {
            return new List<ProposalDto>();
        }

        var proposalIndexList = new List<ProposalDto>(explorerProposalList.Count);
        proposalIndexList.AddRange(explorerProposalList.Select(explorerProposal =>
        {
            long.TryParse(explorerProposal.OrganizationInfo?.ReleaseThreshold?.MinimalVoteThreshold,
                out long minimalRequiredThreshold);
            long.TryParse(explorerProposal.OrganizationInfo?.ReleaseThreshold?.MinimalVoteThreshold,
                out long minimalVoteThreshold);
            long.TryParse(explorerProposal.OrganizationInfo?.ReleaseThreshold?.MinimalApprovalThreshold,
                out long minimalApproveThreshold);
            long.TryParse(explorerProposal.OrganizationInfo?.ReleaseThreshold?.MinimalApprovalThreshold,
                out long maximalRejectionThreshold);
            long.TryParse(explorerProposal.OrganizationInfo?.ReleaseThreshold?.MinimalApprovalThreshold,
                out long maximalAbstentionThreshold);
            long.TryParse(explorerProposal.Approvals, out long approvals);
            long.TryParse(explorerProposal.Abstentions, out long abstentions);
            return new ProposalDto
            {
                ChainId = input.ChainId,
                DAOId = input.DaoId,
                ProposalId = explorerProposal.ProposalId,
                ProposalTitle = explorerProposal.ProposalId,
                ProposalDescription = null,
                ForumUrl = string.Empty,
                ProposalType = "ON-CHAIN",
                ActiveStartTime = explorerProposal.CreateAt,
                ActiveEndTime = default,
                ExecuteStartTime = default,
                ExecuteEndTime = explorerProposal.ExpiredTime,
                //ProposalStatus = ProposalStatus.Empty,
                //ProposalStage = ProposalStage.Default,
                ProposalStatusForOnChain = explorerProposal.Status,
                Proposer = explorerProposal.Proposer,
                SchemeAddress = null,
                Transaction = new ExecuteTransaction
                {
                    ToAddress = explorerProposal.ContractAddress,
                    ContractMethodName = explorerProposal.ContractMethod,
                },
                VoteSchemeId = null,
                VoteMechanismName = null,
                VetoProposalId = null,
                DeployTime = explorerProposal.CreateAt,
                ExecuteTime = explorerProposal.ReleasedTime,
                GovernanceMechanism = GovernanceMechanism.HighCouncil.ToString(),
                MinimalRequiredThreshold = minimalRequiredThreshold,
                MinimalVoteThreshold = minimalVoteThreshold,
                MinimalApproveThreshold = minimalApproveThreshold,
                MaximalRejectionThreshold = maximalRejectionThreshold,
                MaximalAbstentionThreshold = maximalAbstentionThreshold,
                VoterCount = approvals + explorerProposal.Rejections + abstentions,
                VotesAmount = approvals + explorerProposal.Rejections + abstentions,
                ApprovedCount = approvals,
                RejectionCount = explorerProposal.Rejections,
                AbstentionCount = abstentions,
                Decimals = DecimalString,
                Symbol = Symbol,
                ProposalSource = proposalSource,
            };
        }));
        return proposalIndexList;
    }


    public async Task<ProposalDetailDto> QueryProposalDetailAsync(QueryProposalDetailInput input)
    {
        _logger.LogInformation("ProposalService QueryProposalDetailAsync daoid:{ProposalId} start", input.ProposalId);
        var proposalIndex = await _proposalProvider.GetProposalByIdAsync(input.ChainId, input.ProposalId);
        if (proposalIndex == null)
        {
            return new ProposalDetailDto();
        }

        _logger.LogInformation(
            "ProposalService QueryProposalDetailAsync daoid:{ProposalId} proposalIndex {proposalIndex}:",
            input.ProposalId, JsonConvert.SerializeObject(proposalIndex));

        var proposalDetailDto = _objectMapper.Map<ProposalIndex, ProposalDetailDto>(proposalIndex);
        var voteSchemeDic =
            (await _voteProvider.GetVoteSchemeAsync(new GetVoteSchemeInput { ChainId = input.ChainId })).ToDictionary(
                x => x.VoteSchemeId, x => x.VoteMechanism);
        if (voteSchemeDic.TryGetValue(proposalDetailDto.VoteSchemeId, out var voteMechanism))
        {
            proposalDetailDto.VoteMechanismName = voteMechanism.ToString();
        }

        var daoIndex = await _DAOProvider.GetAsync(new GetDAOInfoInput
        {
            ChainId = input.ChainId,
            DAOId = proposalDetailDto.DAOId
        });
        var symbol = daoIndex?.GovernanceToken ?? string.Empty;
        var symbolDecimal = symbol.IsNullOrEmpty()
            ? string.Empty
            : (await _explorerProvider.GetTokenInfoAsync(input.ChainId, new ExplorerTokenInfoRequest
            {
                Symbol = symbol
            })).Decimals;
        proposalDetailDto.Symbol = symbol;
        proposalDetailDto.Decimals = symbolDecimal;
        proposalDetailDto.ProposalLifeList = _proposalAssistService.ConvertProposalLifeList(proposalIndex);
        var voteInfos = await _voteProvider.GetVoteItemsAsync(input.ChainId, new List<string> { input.ProposalId });
        _logger.LogInformation("ProposalService QueryProposalDetailAsync daoid:{ProposalId} voteInfos {voteInfos}:",
            input.ProposalId, JsonConvert.SerializeObject(voteInfos));
        if (voteInfos.TryGetValue(input.ProposalId, out var voteInfo))
        {
            _objectMapper.Map(voteInfo, proposalDetailDto);
        }

        var voteRecords = await _voteProvider.GetVoteRecordAsync(new GetVoteRecordInput
        {
            ChainId = input.ChainId,
            VotingItemId = input.ProposalId,
            Sorting = VoteTopSorting
        });
        _logger.LogInformation("ProposalService QueryProposalDetailAsync daoid:{ProposalId} voteRecords {voteRecords}:",
            input.ProposalId, JsonConvert.SerializeObject(voteRecords));
        proposalDetailDto.VoteTopList = _objectMapper.Map<List<IndexerVoteRecord>, List<VoteRecordDto>>(voteRecords);
        if (proposalDetailDto.ProposalType == ProposalType.Advisory.ToString())
        {
            proposalDetailDto.ExecuteStartTime = null;
            proposalDetailDto.ExecuteEndTime = null;
            proposalDetailDto.ExecuteTime = null;
        }

        if (proposalDetailDto.ProposalStatus != ProposalStatus.Executed.ToString())
        {
            proposalDetailDto.ExecuteTime = null;
        }

        return proposalDetailDto;
    }

    public async Task<MyProposalDto> QueryMyInfoAsync(QueryMyProposalInput input)
    {
        return string.IsNullOrEmpty(input.ProposalId)
            ? await QueryDaoMyInfoAsync(input)
            : await QueryProposalMyInfoAsync(input);
    }

    private async Task<MyProposalDto> QueryProposalMyInfoAsync(QueryMyProposalInput input)
    {
        _logger.LogInformation("ProposalService QueryProposalMyInfoAsync start daoid:{DAOId}:", input.DAOId);
        var proposalIndex = await _proposalProvider.GetProposalByIdAsync(input.ChainId, input.ProposalId);
        _logger.LogInformation(
            "ProposalService QueryProposalMyInfoAsync  daoid:{DAOId}: proposalIndex:{proposalIndex}:", input.DAOId,
            JsonConvert.SerializeObject(proposalIndex));
        if (proposalIndex == null)
        {
            return new MyProposalDto();
        }

        var myProposalDto = _objectMapper.Map<ProposalIndex, MyProposalDto>(proposalIndex);
        _logger.LogInformation("ProposalService QueryProposalMyInfoAsync daoid:{DAOId} myProposalDto:{myProposalDto}:",
            input.DAOId, JsonConvert.SerializeObject(myProposalDto));
        var daoIndex = await _DAOProvider.GetAsync(new GetDAOInfoInput
        {
            ChainId = input.ChainId,
            DAOId = proposalIndex.DAOId
        });
        _logger.LogInformation("ProposalService QueryProposalMyInfoAsync daoid:{DAOId} daoIndex:{daoIndex}:",
            input.DAOId, JsonConvert.SerializeObject(daoIndex));
        myProposalDto.Symbol = daoIndex?.GovernanceToken ?? string.Empty;
        myProposalDto.Decimal = myProposalDto.Symbol.IsNullOrEmpty()
            ? string.Empty
            : (await _explorerProvider.GetTokenInfoAsync(input.ChainId, new ExplorerTokenInfoRequest
            {
                Symbol = myProposalDto.Symbol
            })).Decimals;

        var voteRecords = await _voteProvider.GetVoteRecordAsync(new GetVoteRecordInput
        {
            ChainId = input.ChainId,
            VotingItemId = input.ProposalId,
            Sorting = VoteTopSorting,
            Voter = input.Address
        });
        _logger.LogInformation("ProposalService QueryProposalMyInfoAsync daoid:{DAOId} voteRecords {voteRecords}:",
            input.DAOId, JsonConvert.SerializeObject(voteRecords));
        foreach (var voteRecord in voteRecords)
        {
            _logger.LogInformation("ProposalService QueryProposalMyInfoAsync daoid:{DAOId} voteRecord:{voteRecord}:",
                input.DAOId, JsonConvert.SerializeObject(voteRecord));
            if (voteRecord.VoteMechanism == VoteMechanism.TOKEN_BALLOT)
            {
                myProposalDto.StakeAmount += voteRecord.Amount;
            }

            myProposalDto.VotesAmount = myProposalDto.StakeAmount;
        }

        myProposalDto.CanVote = proposalIndex.ProposalStage == ProposalStage.Active;
        var withdrawnInfos = await _voteProvider.GetVoteWithdrawnAsync(input.ChainId, input.DAOId, input.Address);
        _logger.LogInformation(
            "ProposalService QueryProposalMyInfoAsync daoid:{DAOId} withdrawnInfos:{withdrawnInfos}:", input.DAOId,
            JsonConvert.SerializeObject(withdrawnInfos));
        var hasWithdrawn = false;
        foreach (var withdrawnInfo in withdrawnInfos)
        {
            if (withdrawnInfo.VotingItemIdList.Contains(input.ProposalId))
            {
                hasWithdrawn = true;
            }
        }

        var cutTime = DateTime.Now;
        if (cutTime > proposalIndex.ActiveEndTime && !hasWithdrawn)
        {
            myProposalDto.AvailableUnStakeAmount = myProposalDto.StakeAmount;
        }

        _logger.LogInformation("ProposalService QueryProposalMyInfoAsync daoid:{DAOId} myProposalDto {myProposalDto}:",
            input.DAOId, JsonConvert.SerializeObject(myProposalDto));
        var proposalIdList = new List<string> { };
        proposalIdList.Add(input.ProposalId);

        myProposalDto.ProposalIdList = proposalIdList;
        return myProposalDto;
    }

    private async Task<MyProposalDto> QueryDaoMyInfoAsync(QueryMyProposalInput input)
    {
        _logger.LogInformation("ProposalService QueryDaoMyInfoAsync daoid:{DAOId} start ", input.DAOId);
        var proposalList = await _proposalProvider.GetProposalByDAOIdAsync(input.ChainId, input.DAOId);
        _logger.LogInformation("ProposalService QueryDaoMyInfoAsync daoid:{DAOId} proposalList {proposalList}:",
            input.DAOId, JsonConvert.SerializeObject(proposalList));
        var myProposalDto = new MyProposalDto
        {
            ChainId = input.ChainId
        };
        if (proposalList.IsNullOrEmpty())
        {
            return myProposalDto;
        }

        var daoIndex = await _DAOProvider.GetAsync(new GetDAOInfoInput
        {
            ChainId = input.ChainId,
            DAOId = input.DAOId
        });
        _logger.LogInformation("ProposalService QueryDaoMyInfoAsync daoid:{DAOId} daoIndex {daoIndex}:", input.DAOId,
            JsonConvert.SerializeObject(daoIndex));
        myProposalDto.Symbol = daoIndex?.GovernanceToken ?? string.Empty;
        myProposalDto.Decimal = myProposalDto.Symbol.IsNullOrEmpty()
            ? string.Empty
            : (await _explorerProvider.GetTokenInfoAsync(input.ChainId, new ExplorerTokenInfoRequest
            {
                Symbol = myProposalDto.Symbol
            })).Decimals;
        myProposalDto.CanVote = false;
        var proposalIdList = new List<string> { };
        foreach (var proposalIndex in proposalList)
        {
            var voteRecords = await _voteProvider.GetVoteRecordAsync(new GetVoteRecordInput
            {
                ChainId = input.ChainId,
                VotingItemId = proposalIndex.ProposalId,
                Sorting = VoteTopSorting,
                Voter = input.Address
            });
            _logger.LogInformation("ProposalService QueryDaoMyInfoAsync daoid:{DAOId} voteRecords {voteRecords}:",
                input.DAOId, JsonConvert.SerializeObject(voteRecords));
            var currentStakeAmount = 0;
            foreach (var voteRecord in voteRecords)
            {
                _logger.LogInformation(
                    "ProposalService QueryDaoMyInfoAsync daoid:{DAOId} in count voteRecorditem{voteRecord}",
                    input.DAOId, voteRecord);
                if (voteRecord.VoteMechanism == VoteMechanism.TOKEN_BALLOT)
                {
                    currentStakeAmount += voteRecord.Amount;
                }
            }

            _logger.LogInformation("ProposalService QueryDaoMyInfoAsync daoid:{DAOId} out count", input.DAOId);
            proposalIdList.Add(proposalIndex.ProposalId);
            _logger.LogInformation("ProposalService QueryDaoMyInfoAsync daoid:{DAOId} out1 count", input.DAOId);
            var withdrawnInfos = await _voteProvider.GetVoteWithdrawnAsync(input.ChainId, input.DAOId, input.Address);
            _logger.LogInformation("ProposalService QueryDaoMyInfoAsync daoid:{DAOId} withdrawnInfos:{withdrawnInfos}:",
                input.DAOId, JsonConvert.SerializeObject(withdrawnInfos));
            var hasWithdrawn = false;
            foreach (var withdrawnInfo in withdrawnInfos)
            {
                if (withdrawnInfo.VotingItemIdList.Contains(input.ProposalId))
                {
                    hasWithdrawn = true;
                }
            }

            var cutTime = DateTime.Now;
            myProposalDto.StakeAmount += currentStakeAmount;
            myProposalDto.VotesAmount = myProposalDto.StakeAmount;
            if (cutTime > proposalIndex.ActiveEndTime && !hasWithdrawn)
            {
                myProposalDto.AvailableUnStakeAmount += currentStakeAmount;
            }

            _logger.LogInformation("ProposalService QueryDaoMyInfoAsync daoid:{DAOId} out2 count", input.DAOId);
        }

        myProposalDto.ProposalIdList = proposalIdList;
        _logger.LogInformation("ProposalService QueryDaoMyInfoAsync daoid:{DAOId} end myProposalDto {myProposalDto}:",
            input.DAOId, JsonConvert.SerializeObject(myProposalDto));
        return myProposalDto;
    }

    private async Task<VoteHistoryDto> QueryVoteRecordsAsync(QueryVoteHistoryInput input)
    {
        _logger.LogInformation("ProposalService QueryVoteRecordsAsync daoid:{DAOId} start ", input.Address);
        var myProposalDto = new VoteHistoryDto
        {
            ChainId = input.ChainId,
            Items = new List<IndexerVoteHistoryDto> { }
        };
        var voteRecords = await _voteProvider.GetAddressVoteRecordAsync(new GetVoteRecordInput
        {
            ChainId = input.ChainId,
            VotingItemId = input.ProposalId,
            Sorting = VoteTopSorting,
            Voter = input.Address
        });
        _logger.LogInformation("ProposalService QueryVoteRecordsAsync daoid:{DAOId} voteRecords:{voteRecords} ",
            input.Address, JsonConvert.SerializeObject(voteRecords));
        var votingItemIds = new List<string> { };
        foreach (var voteRecordItem in voteRecords)
        {
            var indexerVoteHistory = new IndexerVoteHistoryDto
            {
                TimeStamp = voteRecordItem.VoteTime,
                ProposalId = voteRecordItem.VotingItemId,
                MyOption = voteRecordItem.Option,
                VoteNum = voteRecordItem.Amount,
                TransactionId = voteRecordItem.TransactionId,
            };
            votingItemIds.Add(voteRecordItem.VotingItemId);
        }

        var voteInfos = await _voteProvider.GetVoteItemsAsync(input.ChainId, votingItemIds);
        _logger.LogInformation("ProposalService QueryVoteRecordsAsync daoid:{DAOId} voteInfos:{voteInfos} ",
            input.Address, JsonConvert.SerializeObject(voteInfos));
        foreach (var voteRecordItem in voteRecords)
        {
            var indexerVoteHistory = new IndexerVoteHistoryDto
            {
                TimeStamp = voteRecordItem.VoteTime,
                ProposalId = voteRecordItem.VotingItemId,
                MyOption = voteRecordItem.Option,
                VoteNum = voteRecordItem.Amount,
                TransactionId = voteRecordItem.TransactionId,
            };
            if (voteInfos.TryGetValue(voteRecordItem.VotingItemId, out var voteInfo))
            {
                indexerVoteHistory.Executer = voteInfo.Executer;
            }

            var proposalIndex =
                await _proposalProvider.GetProposalByIdAsync(input.ChainId, voteRecordItem.VotingItemId);
            if (proposalIndex != null)
            {
                indexerVoteHistory.ProposalTitle = proposalIndex.ProposalTitle;
            }

            _logger.LogInformation("ProposalService QueryVoteRecordsAsync daoid:{DAOId} proposalIndex:{proposalIndex} ",
                input.Address, JsonConvert.SerializeObject(proposalIndex));

            myProposalDto.Items.Add(indexerVoteHistory);
        }

        return myProposalDto;
    }

    public async Task<VoteHistoryDto> QueryVoteHistoryAsync(QueryVoteHistoryInput input)
    {
        return await QueryVoteRecordsAsync(input);
    }
}