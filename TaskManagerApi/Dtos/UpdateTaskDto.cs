using System.ComponentModel.DataAnnotations;
using TaskManagerApi.Models;

namespace TaskManagerApi.Dtos;

// للـ PATCH: كل الحقول ممكن تكون null (يعني ما انبعتت)
public class UpdateTaskDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters")]
    public string? Title { get; set; }

    [StringLength(1000, ErrorMessage = "Description can't be longer than 1000 characters")]
    public string? Description { get; set; }

    [EnumDataType(typeof(TaskItemStatus), ErrorMessage = "Status must be Todo, InProgress or Done")]
    public TaskItemStatus? Status { get; set; }

    [EnumDataType(typeof(TaskPriority), ErrorMessage = "Priority must be Low, Medium or High")]
    public TaskPriority? Priority { get; set; }

    public DateOnly? DueDate { get; set; }
}