using System.Text.Json.Serialization;

namespace TodoRESTApi.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum TodoStatus
{
    Pending,
    InProgress,
    Completed,
    Archived
}

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum TodoPriority
{
    Low,
    Medium,
    High
}

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum TodoSortField
{
    Name,
    Description,
    Category,
    Status,
    DueDate,
    Priority,
    Id // Default sort field if needed
}

