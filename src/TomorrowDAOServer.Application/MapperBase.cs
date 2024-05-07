using System.Collections.Generic;
using AutoMapper;

namespace TomorrowDAOServer;

public class MapperBase : Profile
{
    protected static Dictionary<string, object> MapTransactionParams(string param)
    {
        return new Dictionary<string, object>
        {
            ["param"] = string.IsNullOrEmpty(param) ? "" : param
        };
    }
}