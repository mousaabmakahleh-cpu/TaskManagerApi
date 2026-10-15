using TaskManagerApi.Models;

namespace TaskManagerApi.Dtos;

// اللي بيرجعه الـ API للـ client
public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProjectId { get; set; }
}