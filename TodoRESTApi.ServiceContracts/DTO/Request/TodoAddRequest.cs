using System.ComponentModel.DataAnnotations;
using TodoRESTApi.Core.Enums;
using TodoRESTApi.Entities.Entities;

namespace TodoRESTApi.ServiceContracts.DTO.Request;

/// <summary>
/// Represents the request model for adding a new Todo item
/// </summary>
public class TodoAddRequest
{
    [Required(ErrorMessage = "The Name field is required.")]
    [MaxLength(100, ErrorMessage = "Name can't exceed 100 characters")]
    public string Name { get; set; } = null!;
        
    [MaxLength(500, ErrorMessage = "Description can't exceed 500 characters")]
    public string? Description { get; set; }
        
    [Required(ErrorMessage = "The Due Date field is required.")]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }
        
    [Required(ErrorMessage = "The Status field is required.")]
    [EnumDataType(typeof(TodoStatus), ErrorMessage = "Invalid Todo Status")]
    public TodoStatus Status { get; set; } = TodoStatus.Pending;
        
    [Required(ErrorMessage = "The Priority field is required.")]
    [EnumDataType(typeof(TodoPriority), ErrorMessage = "Invalid Todo Priority")]
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
        
    [MaxLength(50, ErrorMessage = "Category can't exceed 50 characters")]
    public string? Category { get; set; }
    
    /// <summary>
    /// Converts the TodoAddRequest to a Todo entity.
    /// </summary>
    /// <returns>A new Todo entity.</returns>
    public Todo ToTodo()
    {
        return new Todo
        {
            Name = this.Name,
            Description = this.Description,
            DueDate = this.DueDate,
            Status = this.Status,
            Priority = this.Priority,
            Category = this.Category,
            IsDeleted = false
        };
    }
}