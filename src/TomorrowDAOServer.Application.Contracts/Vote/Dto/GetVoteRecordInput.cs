namespace TomorrowDAOServer.Vote.Dto;

public class GetVoteRecordInput
{
    public string ChainId { get; set; }

    public string VotingItemId { get; set; }
    
    public string Voter { get; set; }
    
    public string Sorting { get; set; }
}