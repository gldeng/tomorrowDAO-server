using System.ComponentModel.DataAnnotations;

namespace TomorrowDAOServer.DAO.Dtos;

public class GetHcMemberInput : GetDAOInfoInput
{
    public string Type { get; set; }
    public string Sorting { get; set; }
    [Range(0, int.MaxValue)] 
    public int SkipCount { get; set; } = 0;
    [Range(1, int.MaxValue)]
    public int MaxResultCount { get; set; } = 20;
}