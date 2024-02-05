using System.ComponentModel.DataAnnotations;

namespace TomorrowDAOServer.Dtos.DAO;

public class GetDAORequestDto
{
    [Required]
    public string ChainId { get; set; }
    [Required]
    public string DAOId { get; set; }
}