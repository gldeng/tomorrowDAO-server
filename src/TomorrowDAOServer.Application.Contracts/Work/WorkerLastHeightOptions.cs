using System.Collections.Generic;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Enums;

namespace TomorrowDAOServer.Work;

public class WorkerLastHeightOptions
{
    public IDictionary<string, IDictionary<string, long>> WorkerLastHeight { get; set; }

    public IDictionary<string, long> GetHeightSettings(WorkerBusinessType businessType)
    {
        return WorkerLastHeight?.GetOrDefault(businessType.ToString()) ??
               new Dictionary<string, long>();
    }
}