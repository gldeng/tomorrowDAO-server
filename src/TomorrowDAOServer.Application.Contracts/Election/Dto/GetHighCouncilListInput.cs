using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace TomorrowDAOServer.Election.Dto;

public class GetHighCouncilListInput : PagedResultRequestDto
{
    [Required] public string ChainId { get; set; }
    public string DAOId { get; set; }
    public string HighCouncilType { get; set; }
    public long TermNumber { get; set; }
}