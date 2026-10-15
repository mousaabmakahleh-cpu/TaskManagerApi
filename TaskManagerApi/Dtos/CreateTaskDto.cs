using System.ComponentModel.DataAnnotations;
using TaskManagerApi.Models;

namespace TaskManagerApi.Dtos;

// اللي بيبعته الـ client لما يضيف مهمة
// ما فيه ProjectId، لأنه بييجي من الرابط
public class CreateTaskDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters")]
    public string Title { get; set; } = "";

    [StringLength(1000, ErrorMessage = "Description can't be longer than 1000 characters")]
    public string? Description { get; set; }

    // EnumDataType: بيتأكد إنه القيمة من القيم المسموحة بالـ Enum
    [EnumDataType(typeof(TaskItemStatus), ErrorMessage = "Status must be Todo, InProgress or Done")]
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

    [EnumDataType(typeof(TaskPriority), ErrorMessage = "Priority must be Low, Medium or High")]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    // اختياري، بصيغة "2026-10-20"
    public DateOnly? DueDate { get; set; }
}