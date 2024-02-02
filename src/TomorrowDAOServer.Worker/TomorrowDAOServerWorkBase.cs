using TomorrowDAOServer.Common;
using TomorrowDAOServer.Work;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Enums;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace TomorrowDAOServer.Worker;

public abstract class TomorrowDAOServerWorkBase : AsyncPeriodicBackgroundWorkerBase
{
    protected abstract WorkerBusinessType BusinessType { get; }

    protected readonly ILogger<ScheduleSyncDataContext> _logger;
    protected readonly IScheduleSyncDataContext _scheduleSyncDataContext;
 
    protected TomorrowDAOServerWorkBase(ILogger<ScheduleSyncDataContext> logger,
        AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IScheduleSyncDataContext scheduleSyncDataContext,
        IOptionsMonitor<WorkerOptions> optionsMonitor) :
        base(timer, serviceScopeFactory)
    {
        _logger = logger;
        _scheduleSyncDataContext = scheduleSyncDataContext;
        timer.Period = optionsMonitor.CurrentValue.GetWorkerSettings(BusinessType).TimePeriod;
        
        //to change timer Period if the WorkerOptions has changed.
        optionsMonitor.OnChange((newOptions, _) =>
        {
            var workerSetting = newOptions.GetWorkerSettings(BusinessType);
            timer.Period = workerSetting.TimePeriod;
            if (workerSetting.OpenSwitch)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }

            _logger.LogInformation(
                "The workerSetting of Worker {BusinessType} has changed to Period = {Period} ms, OpenSwitch = {OpenSwitch}.",
                BusinessType, timer.Period, workerSetting.OpenSwitch);
        });
    }

}