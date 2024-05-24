using System.ComponentModel.DataAnnotations;

namespace TomorrowDAOServer.Dao.Dto;

public class GetContractInfoInput
{
    [Required] public string ChainId { get; set; }
    
    [Required] public string ContractAddress { get; set; }
}
