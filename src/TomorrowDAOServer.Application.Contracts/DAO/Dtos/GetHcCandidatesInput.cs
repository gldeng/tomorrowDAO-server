using System.ComponentModel.DataAnnotations;

namespace TomorrowDAOServer.DAO.Dtos;

public class GetHcCandidatesInput : GetDAOInfoInput
{
    [Range(0, int.MaxValue)]
    public virtual int SkipCount { get; set; }
    
    [Range(1, int.MaxValue)]
    public int MaxResultCount { get; set; }
}