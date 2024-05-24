using System.Collections.Generic;
using NSubstitute;
using Shouldly;
using TomorrowDAOServer.Contract.Provider;
using TomorrowDAOServer.Options;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace TomorrowDAOServer.Contract;

public class ContractServiceTest
{ 
    private static readonly IContractProvider ContractProvider = Substitute.For<IContractProvider>();
    private readonly ContractService _contractService = new(Substitute.For<IObjectMapper>(), ContractProvider);

    [Fact]
    public void GetContractInfo_Test()
    {
        var result = _contractService.GetContractInfo("AELF");
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public  void GetFunctionList_Test()
    {
        ContractProvider.GetContractInfo(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new List<ContractInfo>());
        
        var result = _contractService.GetFunctionList("AELF", "contractAddress");
        result.ShouldNotBeNull();
    }
}