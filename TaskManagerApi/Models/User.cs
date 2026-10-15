namespace TaskManagerApi.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";

    // الباسوورد بعد التشفير، مش الأصلي أبداً
    public string PasswordHash { get; set; } = "";

    //متى المستخدم سجل حسابه
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property: المستخدم إله مشاريع كثير
    public List<Project> Projects { get; set; } = new List<Project>();
}