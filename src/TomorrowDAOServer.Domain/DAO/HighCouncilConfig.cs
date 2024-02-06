namespace TomorrowDAOServer.DAO;

public class HighCouncilConfig
{
    public int MaxHighCouncilMemberCount { get; set; }
    public int MaxHighCouncilCandidateCount { get; set; }
    public int ElectionPeriod { get; set; }
    public bool IsRequireHighCouncilForExecution { get; set; }
}