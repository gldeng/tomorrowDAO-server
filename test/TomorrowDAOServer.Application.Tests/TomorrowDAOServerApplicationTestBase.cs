using Xunit.Abstractions;

namespace TomorrowDAOServer;

public abstract partial class TomorrowDAOServerApplicationTestBase : TomorrowDAOServerOrleansTestBase<TomorrowDAOServerApplicationTestModule>
{

    public  readonly ITestOutputHelper Output;
    protected TomorrowDAOServerApplicationTestBase(ITestOutputHelper output) : base(output)
    {
        Output = output;
    }
}