using Xunit.Abstractions;

namespace TomorrowDAOServer;

public abstract partial class TomorrowDAOServerApplicationTestBase : TomorrowDAOServerOrleansTestBase<TomorrowDAOServerApplicationTestModule>
{
    protected const string ChainID = "tDVV";
    protected const string ELF = "ELF";
    
    public  readonly ITestOutputHelper Output;
    protected TomorrowDAOServerApplicationTestBase(ITestOutputHelper output) : base(output)
    {
        Output = output;
    }
}