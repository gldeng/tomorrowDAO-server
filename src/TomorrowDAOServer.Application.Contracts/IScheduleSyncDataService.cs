using System.Collections.Generic;
using System.Threading.Tasks;
using TomorrowDAOServer.Common;

namespace TomorrowDAOServer;

public interface IScheduleSyncDataService
{
    Task<long> SyncIndexerRecordsAsync(string chainId,long lastEndHeight, long newIndexHeight);
     
    Task<List<string>> GetChainIdsAsync();

    WorkerBusinessType GetBusinessType();
    
    Task DealDataAsync();
}