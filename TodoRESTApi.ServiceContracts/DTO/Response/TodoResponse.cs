using TodoRESTApi.Core.Enums;
using TodoRESTApi.Entities.Entities;

namespace TodoRESTApi.ServiceContracts.DTO.Response;

/// <summary>
/// Represents the response model for a Todo item
/// </summary>
public class TodoResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public TodoStatus Status { get; set; }
    public TodoPriority Priority { get; set; }
    public string? Category { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Provides extension methods for the Todo entity.
/// </summary>
public static class TodoExtensions
{
    /// <summary>
    /// Converts a Todo entity to a TodoResponse DTO.
    /// </summary>
    /// <param name="todo">The Todo entity to convert.</param>
    /// <returns>A TodoResponse object representing the Todo entity.</returns>
    public static TodoResponse ToTodoResponse(this Todo todo)
    {
        return new TodoResponse()
        {
            Id = todo.Id,                     
            Name = todo.Name,               
            Description = todo.Description,    
            DueDate = todo.DueDate,            
            Status = todo.Status,            
            Priority = todo.Priority,     
            Category = todo.Category,         
            IsDeleted = todo.IsDeleted         
        };
    }
}
