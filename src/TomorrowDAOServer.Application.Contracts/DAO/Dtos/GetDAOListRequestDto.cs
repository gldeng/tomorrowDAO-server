using System.ComponentModel.DataAnnotations;

namespace TomorrowDAOServer.DAO.Dtos;

public class GetDAOListRequestDto
{
    [Required] public string ChainId { get; set; }
    
    [Range(0, int.MaxValue)]
    public virtual int SkipCount { get; set; }
    
    [Range(1, int.MaxValue)]
    public int MaxResultCount { get; set; } = 6;
}