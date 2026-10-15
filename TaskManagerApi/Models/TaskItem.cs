namespace TaskManagerApi.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    // DateOnly: تاريخ بس بدون ساعة، والـ ? يعني اختياري
    //موعد التسليم
    public DateOnly? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key: رقم المشروع اللي فيه هالمهمة
    public int ProjectId { get; set; }

    // Navigation Property: المشروع نفسه
    public Project? Project { get; set; }
}