using System.Collections.Generic;
using Nest;

namespace TomorrowDAOServer.DAO;

public class DAOMetadata
{
    [Keyword] public string Name { get; set; }
    [Keyword] public string LogoUrl { get; set; }
    [Keyword] public string Description { get; set; }
    public Dictionary<string, string> SocialMedia { get; set; }
}