using Nest;

namespace TomorrowDAOServer.DAO;

public class FileInfo
{
    [Keyword] public string Name { get; set; }
    [Keyword] public string Hash { get; set; }
    [Keyword] public string Url { get; set; }
}