using System;

namespace TomorrowDAOServer.Token;

public class Token : TokenBasicInfo
{
    public Guid ChainId { get; set; }
}