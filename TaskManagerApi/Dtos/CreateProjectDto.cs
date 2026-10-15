using System.ComponentModel.DataAnnotations;

namespace TaskManagerApi.Dtos;

// اللي بيبعته الـ client لما يضيف أو يعدل مشروع
// لاحظ: ما فيه UserId، لأنه بناخده من التوكن مش من الـ client
public class CreateProjectDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = "";

    [StringLength(500, ErrorMessage = "Description can't be longer than 500 characters")]
    public string? Description { get; set; }
}