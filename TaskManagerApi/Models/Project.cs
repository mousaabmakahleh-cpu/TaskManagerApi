namespace TaskManagerApi.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key: رقم صاحب المشروع
    public int UserId { get; set; }

    // Navigation Property: صاحب المشروع نفسه
    public User? User { get; set; }

    // Navigation Property: المشروع فيه مهام كثير
    public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}