using System;
using AElf.Indexing.Elasticsearch;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TomorrowDAOServer.Enums;

namespace TomorrowDAOServer.Entities;

public class UserTaskIndex : AbstractEntity<string>, IIndexBuild
{
    [Keyword] public override string Id { get; set; }
    [Keyword] public string ChainId { get; set; }
    [Keyword] public string Address { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public UserTask UserTask { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public UserTaskDetail UserTaskDetail { get; set; }
    public long Points { get; set; }
    public DateTime CompleteTime { get; set; }
}