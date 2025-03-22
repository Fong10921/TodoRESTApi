using System.ComponentModel.DataAnnotations;

namespace TodoRESTApi.Core.Enums;

public enum TodoStatus
{
    [Display(Name = "Pending Task")]
    Pending,

    [Display(Name = "In Progress")]
    InProgress,

    [Display(Name = "Completed Task")]
    Completed,

    [Display(Name = "Archived Task")]
    Archived
}

public enum TodoPriority
{
    [Display(Name = "Low Priority")]
    Low,

    [Display(Name = "Medium Priority")]
    Medium,

    [Display(Name = "High Priority")]
    High
}
