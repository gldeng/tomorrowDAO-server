using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomorrowDAOServer.Common;

namespace TomorrowDAOServer.Proposal.Dto;

public class QueryVoteHistoryInput : IValidatableObject
{
    [Required] public string ChainId { get; set; }
    // [Required] public string DAOId { get; set; }
    [Required] public string Address { get; set; }
    public string ProposalId { get; set; }
    public string SkipCount { get; set; }
    public string MaxResultCount { get; set; }
    public string VoteOption { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ChainId.IsNullOrEmpty() || !ChainId.MatchesChainId())
        {
            yield return new ValidationResult($"ChainId invalid.");
        }
    }
}