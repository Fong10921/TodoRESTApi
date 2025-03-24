using TodoRESTApi.Core.Enums;

namespace TodoRESTApi.ServiceContracts.Filters;

public class TodoFilters
{
    public int? TodoId { get; set; }
    
    public string? Name { get; set; }
    
    public DateTime? FromDueDate { get; set; }
    
    public DateTime? ToDueDate { get; set; }
    
    public string? Category { get; set; }
    
    public TodoPriority? Priority { get; set; }
    
    public TodoStatus? Status { get; set; }
    
    /// <summary>
    /// Determines if all todos should be fetched when no filters are provided.
    /// </summary>
    public bool? GetAll { get; set; } = false;
}