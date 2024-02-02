using System.Collections.Generic;

namespace TomorrowDAOServer.Dtos;

public abstract class PageResult<T>
{
    protected PageResult(long total, List<T> data)
    {
        TotalCount = total;
        Data = data;
    }
    
    public long TotalCount { get; set; }
    public List<T> Data { get; set; }
    
}