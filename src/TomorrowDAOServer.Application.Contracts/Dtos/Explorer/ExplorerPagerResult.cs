using System.Collections.Generic;

namespace TomorrowDAOServer.Dtos.Explorer;

public class ExplorerPagerResult<T>
{
    
    public long Total { get; set; }
    public List<T> Data { get; set; }
    
}