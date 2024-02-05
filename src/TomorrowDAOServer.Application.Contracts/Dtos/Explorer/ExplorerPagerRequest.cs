namespace TomorrowDAOServer.Dtos.Explorer;

public class ExplorerPagerRequest
{
    public int PageSize { get; set; } = 10;
    public int PageNum { get; set; } = 1;


    protected ExplorerPagerRequest()
    {
    }

    protected ExplorerPagerRequest(int pageNum, int pageSize)
    {
        PageNum = pageNum;
        PageSize = pageSize;
    }
    
    
}