using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using System;

namespace TomorrowDAOServer.Commitment.Dto;

public class QueryByProposalIdInput : PagedResultRequestDto
{
    [Required] public string ProposalId { get; set; }


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ProposalId.IsNullOrEmpty())
        {
            yield return new ValidationResult("ProposalId is required.");
        }
    }
}