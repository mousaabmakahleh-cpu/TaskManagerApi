namespace TaskManagerApi.Dtos;

// اللي بيرجعه الـ API للـ client
// ما بنرجع الـ Project نفسه، عشان ما نكشف UserId ولا نرجع بيانات زيادة
public class ProjectResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TaskCount { get; set; }   // كم مهمة بالمشروع
}