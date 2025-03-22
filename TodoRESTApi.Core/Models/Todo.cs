using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TodoRESTApi.Core.Enums;

namespace TodoRESTApi.Core.Models;

/// <summary>
/// Represents a Todo entity for the view
/// </summary>
public class Todo
{
    [Required]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "The Name field is required.")]
    [MaxLength(100, ErrorMessage = "Name can't exceed 100 characters")]
    public string Name { get; set; }
    
    [MaxLength(500, ErrorMessage = "Description can't exceed 100 characters")]
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

    [MaxLength(50, ErrorMessage = "Category cant exceed 50 characters")]
    public string? Category { get; set; }

    [Display(Name = "Is Deleted")]
    [DefaultValue(false)]
    [Required(ErrorMessage = "The Is Deleted field is required.")]
    public bool IsDeleted { get; set; } = false;
}