using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Nest;
using TomorrowDAOServer.Commitment.Dto;
using TomorrowDAOServer.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.ObjectMapping;

namespace TomorrowDAOServer.Commitment;

public class CommitmentQueryService : ICommitmentQueryService
{
    private readonly IObjectMapper _objectMapper;
    private readonly INESTRepository<CommitmentIndex, string> _commitmentIndexRepository;

    public CommitmentQueryService(IObjectMapper objectMapper,
        INESTRepository<CommitmentIndex, string> commitmentIndexRepository)
    {
        _objectMapper = objectMapper;
        _commitmentIndexRepository = commitmentIndexRepository;
    }

    public async Task<PagedResultDto<CommitmentDto>> QueryCommitmentsByProposalId(QueryByProposalIdInput input)
    {
        if (input == null || input.ProposalId.IsNullOrWhiteSpace())
        {
            ExceptionHelper.ThrowArgumentException();
        }

        var mustQuery = new List<Func<QueryContainerDescriptor<CommitmentIndex>, QueryContainer>>();

        if (!input.ProposalId.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i =>
                i.Field(f => f.ProposalId).Value(input.ProposalId)));
        }

        QueryContainer Filter(QueryContainerDescriptor<CommitmentIndex> f) =>
            f.Bool(b => b.Must(mustQuery)
            );

        //add sorting
        var sortDescriptor = GetDescendingTimestampSortDescriptor();

        var (count, items) = await _commitmentIndexRepository.GetSortListAsync(Filter, sortFunc: sortDescriptor,
            skip: input.SkipCount,
            limit: input.MaxResultCount);

        var dtoList = _objectMapper.Map<List<CommitmentIndex>, List<CommitmentDto>>(items);

        return new PagedResultDto<CommitmentDto>()
        {
            Items = dtoList,
            TotalCount = count,
        };
    }

    public async Task<CommitmentDto> QueryCommitmentByProposalIdAndVoter(QueryByProposalIdAndVoter input)
    {
        if (input == null || (input.ProposalId.IsNullOrWhiteSpace() && input.Voter.IsNullOrWhiteSpace()))
        {
            ExceptionHelper.ThrowArgumentException();
        }

        var mustQuery = new List<Func<QueryContainerDescriptor<CommitmentIndex>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i =>
            i.Field(f => f.ProposalId).Value(input.ProposalId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.Voter).Value(input.Voter)));


        QueryContainer Filter(QueryContainerDescriptor<CommitmentIndex> f) =>
            f.Bool(b => b.Must(mustQuery)
            );

        var res = await _commitmentIndexRepository.GetAsync(Filter);
        var dto = _objectMapper.Map<CommitmentIndex, CommitmentDto>(res);
        return dto;
    }


    private static Func<SortDescriptor<CommitmentIndex>, IPromise<IList<ISort>>> GetDescendingTimestampSortDescriptor()
    {
        //use default
        var sortDescriptor = new SortDescriptor<CommitmentIndex>();
        sortDescriptor.Descending(a => a.Timestamp);
        return _ => sortDescriptor;
    }
}