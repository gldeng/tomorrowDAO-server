using System.ComponentModel.DataAnnotations;

namespace TomorrowDAOServer.DAO.Dtos;

public class GetDAOInfoInput
{
    [Required]
    public string ChainId { get; set; }
    [Required]
    public string DAOId { get; set; }
}