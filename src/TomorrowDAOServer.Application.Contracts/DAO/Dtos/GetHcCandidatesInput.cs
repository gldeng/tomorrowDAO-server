namespace TomorrowDAOServer.DAO.Dtos;

public class GetHcCandidatesInput : GetDAOInfoInput
{
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
}