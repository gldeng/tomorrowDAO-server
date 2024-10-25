using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using AElf.Types;
using JetBrains.Annotations;

namespace TomorrowDAOServer.Commitment.Dto;

public class QueryByProposalIdAndVoter : IValidatableObject
{
    [Required] public string ProposalId { get; set; }
    [Required] public string Voter { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ProposalId.IsNullOrEmpty())
        {
            yield return new ValidationResult("ProposalId is required.");
        }

        var address = GetAddress(Voter);
        if (address == null)
        {
            yield return new ValidationResult("Address is not valid.");
        }
    }

    [CanBeNull]
    private static Address GetAddress(string address)
    {
        try
        {
            return Address.FromBase58(address);
        }
        catch
        {
            return null;
        }
    }
}