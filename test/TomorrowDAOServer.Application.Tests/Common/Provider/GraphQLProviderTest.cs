using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Orleans;
using Shouldly;
using TomorrowDAOServer.Common.GraphQL;
using Xunit;

namespace TomorrowDAOServer.Common.Provider;

public class GraphQLProviderTest
{
    private static readonly IGraphQLClient GraphQlClient = Substitute.For<IGraphQLClient>();
    private static readonly IClusterClient ClusterClient = Substitute.For<IClusterClient>();
    private static readonly ILogger<GraphQLProvider> Logger = Substitute.For<ILogger<GraphQLProvider>>();
    private static readonly IGraphQlClientFactory GraphQlClientFactory = Substitute.For<IGraphQlClientFactory>();
    private readonly GraphQLProvider _graphQlProvider = new(GraphQlClient, Logger, ClusterClient, GraphQlClientFactory);

    [Fact]
    public async void GetHoldersAsync_Test()
    {
        GraphQlClientFactory.GetClient(Arg.Any<GraphQLClientEnum>()).Returns(GraphQlClient);
        GraphQlClient.SendQueryAsync<HolderResult>(Arg.Any<GraphQLRequest>())
            .Returns(Task.FromResult(new GraphQLResponse<HolderResult>
            {
                Data = new HolderResult
                {
                    Data = new List<HolderDto>
                    {
                        new()
                        {
                            HolderCount = 1
                        }
                    }
                }
            }));
        var holders = await _graphQlProvider.GetHoldersAsync("ELF", "AELF", 0, 1);
        holders.ShouldBe(1);
    }
}