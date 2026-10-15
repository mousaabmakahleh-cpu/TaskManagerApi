namespace TaskManagerApi.Dtos;

// اللي بيرجعه الـ API بعد التسجيل أو الدخول
public class AuthResponseDto
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}