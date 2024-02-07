using System.Collections.Generic;
using System.Linq;

namespace TomorrowDAOServer.Options;

public class ProposalTagOptions
{
    //key is methodName, value is tag list.
    public Dictionary<string, List<string>> Mapping { get; set; } = new();

    public List<string> MatchTagList(string methodName)
    {
        if (methodName == null)
        {
            return new List<string>();
        }

        return Mapping.FirstOrDefault(pair => pair.Key.Equals(methodName)).Value ?? new List<string>();
    }
}