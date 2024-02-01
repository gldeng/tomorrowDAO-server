using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TomorrowDAOServer.Common;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer;

public interface IScheduleSyncDataContext
{
    Task DealAsync(WorkerBusinessType businessType);
}

public class ScheduleSyncDataContext : ITransientDependency, IScheduleSyncDataContext
{
    private readonly Dictionary<WorkerBusinessType, IScheduleSyncDataService> _syncDataServiceMap;
    private readonly ILogger<ScheduleSyncDataContext> _logger;

    public ScheduleSyncDataContext(IEnumerable<IScheduleSyncDataService> scheduleSyncDataServices, 
        ILogger<ScheduleSyncDataContext> logger)
    {
        _syncDataServiceMap = scheduleSyncDataServices.ToDictionary(a => a.GetBusinessType(),a => a);
        _logger = logger;
    }

    public async Task DealAsync(WorkerBusinessType businessType)
    {
        var stopwatch = Stopwatch.StartNew();
        await _syncDataServiceMap.GetOrDefault(businessType).DealDataAsync();
        stopwatch.Stop();
        _logger.LogInformation("It took {Elapsed} ms to execute synchronized data for businessType: {businessType}",
            stopwatch.ElapsedMilliseconds, businessType);
    }
}