using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Common.Provider;
using TomorrowDAOServer.DAO.Dtos;
using TomorrowDAOServer.DAO.Indexer;
using TomorrowDAOServer.DAO.Provider;
using TomorrowDAOServer.Election.Dto;
using TomorrowDAOServer.Election.Index;
using TomorrowDAOServer.Election.Provider;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Proposal.Provider;
using Volo.Abp.Application.Dtos;
using Xunit;
using Xunit.Abstractions;

namespace TomorrowDAOServer.DAO;

public class DAOAppServiceTests : TomorrowDAOServerApplicationTestBase
{
    private readonly IDAOAppService _daoAppService;

    public DAOAppServiceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _daoAppService = GetRequiredService<IDAOAppService>();
    }

    protected override void AfterAddApplication(IServiceCollection services)
    {
        services.AddSingleton(MockDAOProvider());
        services.AddSingleton(MockElectionProvider());
        services.AddSingleton(MockProposalProvider());
        services.AddSingleton(MockGraphQlProvider());
        base.AfterAddApplication(services);
    }

    [Fact]
    public async void QueryDAOAsync_Test()
    {
        var result = await _daoAppService.GetDAOByIdAsync(new GetDAOInfoInput
        {
            ChainId = "AELF",
            DAOId = "test1"
        });
        result.ShouldNotBeNull();
        
        var ret = await _daoAppService.GetMemberListAsync(new GetHcMemberInput
        {
            ChainId = "AELF",
            DAOId = "test1",
            Type = HighCouncilType.Member.ToString()
        });
        ret.ShouldNotBeNull();
        
        var list = await _daoAppService.GetDAOListAsync(new QueryDAOListInput
        {
            ChainId = "AELF"
        });
        list.ShouldNotBeNull();
    }

    private IDAOProvider MockDAOProvider()
    {
        var mock = new Mock<IDAOProvider>();

        mock.Setup(p => p.GetSyncDAOListAsync(It.IsAny<GetChainBlockHeightInput>())).ReturnsAsync(
            new List<IndexerDAOInfo>
            {
                new()
                {
                    Id = "test1",
                    ChainId = "AELF",
                    BlockHeight = 100,
                    Creator = "AA1"
                },
                new()
                {
                    Id = "test2",
                    ChainId = "AELF",
                    BlockHeight = 100,
                    Creator = "AA2"
                }
            });

        mock.Setup(p => p.GetAsync(It.IsAny<GetDAOInfoInput>())).ReturnsAsync(
            new DAOIndex
            {
                Id = "test1",
                ChainId = "AELF",
                BlockHeight = 100,
                Creator = "AA1"
            });

        mock.Setup(p => p.GetDAOListAsync(It.IsAny<QueryDAOListInput>())).ReturnsAsync(
            new Tuple<long, List<DAOIndex>>
            (
                2,
                new List<DAOIndex>
                {
                    new()
                    {
                        Id = "test1",
                        ChainId = "AELF",
                        BlockHeight = 100,
                        Creator = "AA1",
                        GovernanceToken = "USDT"
                    },
                    new()
                    {
                        Id = "test2",
                        ChainId = "AELF",
                        BlockHeight = 100,
                        Creator = "AA2",
                        GovernanceToken = "USDT"
                    }
                }
            ));

        return mock.Object;
    }

    private IElectionProvider MockElectionProvider()
    {
        var mock = new Mock<IElectionProvider>();

        mock.Setup(p => p.GetHighCouncilListAsync(It.IsAny<GetHighCouncilListInput>())).ReturnsAsync(
            new PagedResultDto<IndexerElection>(
                1, 
                new List<IndexerElection>
                {
                    new()
                    {
                        ChainId = "AELF",
                        Address = "xxxx"
                    }
                }
            ));

        return mock.Object;
    }
    
    private IProposalProvider MockProposalProvider()
    {
        var mock = new Mock<IProposalProvider>();

        mock.Setup(p => p.GetProposalCountByDAOIds(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(
                1L);

        return mock.Object;
    }

    private IGraphQLProvider MockGraphQlProvider()
    {
        var mock = new Mock<IGraphQLProvider>();

        mock.Setup(p => p.GetHoldersAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(
                1L);

        return mock.Object;
    }
}