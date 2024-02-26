using NSubstitute;
using Shouldly;
using TomorrowDAOServer.Governance.Provider;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace TomorrowDAOServer.Governance;

public class GovernanceServiceTest
{ 
    private readonly GovernanceService _governanceService = new(Substitute.For<IGovernanceProvider>(), Substitute.For<IObjectMapper>());

    [Fact]
    public async void GetGovernanceMechanismAsync_Test()
    {
        var result = await _governanceService.GetGovernanceMechanismAsync("AELF");
        result.ShouldNotBeNull();
    }
}