using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TodoRESTApi.Core.Enums;

namespace TodoRESTApi.Entities.Entities;

public class Todo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public TodoStatus Status { get; set; } = TodoStatus.Pending;
    
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;

    [MaxLength(50)]
    public string? Category { get; set; }

    public bool IsDeleted { get; set; } = false;
}