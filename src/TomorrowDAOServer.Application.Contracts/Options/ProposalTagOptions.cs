using System.Collections.Generic;
using System.Linq;

namespace TomorrowDAOServer.Options;

public class ProposalTagOptions
{
    //key is tag ,value is MethodName or GovernanceMechanism
    public Dictionary<string, List<string>> Mapping { get; set; } = new();

    public Dictionary<string, string> ReverseMapping { get; set; } = new();

    public string MatchTag(string key)
    {
        if (key == null)
        {
            return null;
        }

        if (ReverseMapping.IsNullOrEmpty())
        {
            ToReverse();
        }

        return ReverseMapping.TryGetValue(key, out var tag) ? tag : null;
    }

    private void ToReverse()
    {
        foreach (var kvp in Mapping)
        {
            var key = kvp.Key;
            var values = kvp.Value;
            foreach (var value in values)
            {
                ReverseMapping[value] = key;
            }
        }
    }
}