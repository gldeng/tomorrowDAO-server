using TomorrowDAOServer.Enums;

namespace TomorrowDAOServer.Vote.Dto;

public class VoteRecordDto
{
    public string Voter { get; set; }
    
    public int Amount { get; set; }

    public VoteOption Option { get; set; }
}