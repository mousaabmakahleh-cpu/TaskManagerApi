using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskManagerApi.Data;
using TaskManagerApi.Dtos;
using TaskManagerApi.Models;

namespace TaskManagerApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // POST: api/v1/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        // نخلي الاسم حروف صغيرة، عشان "Mousaab" و "mousaab" يكونوا نفس الشخص
        string username = dto.Username.Trim().ToLower();

        // 1. الاسم مستخدم قبل؟
        bool exists = await _context.Users.AnyAsync(u => u.Username == username);

        if (exists)
        {
            //طلبك بيتعارض مع إشي موجود
            return Conflict(new { message = "Username is already taken" }); //409
        }

        // 2. نشفّر الباسوورد ونخزن الـ hash بس
        User user = new User
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(); // INSERT

        // 3. نرجعله توكن مباشرة، عشان ما يضطر يعمل login بعد التسجيل
        return StatusCode(StatusCodes.Status201Created, CreateAuthResponse(user)); //201
    }

    // POST: api/v1/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        string username = dto.Username.Trim().ToLower();

        // 1. ندور على المستخدم
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        // 2. إذا مش موجود، أو الباسوورد ما بيطابق الـ hash ← نفس الرسالة بالحالتين
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password" }); //401
        }

        // 3. كل إشي تمام، نرجع توكن
        return Ok(CreateAuthResponse(user)); //200
    }

    // private: بنستخدمها جوا الكلاس بس، ومش endpoint
    private AuthResponseDto CreateAuthResponse(User user)
    {
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"]!));

        SymmetricSecurityKey key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                // أهم Claim: رقم المستخدم، رح نستخدمه لنعرف مين صاحب كل مشروع
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Expires = expiresAt,
            SigningCredentials = credentials
        };

        JsonWebTokenHandler handler = new JsonWebTokenHandler();

        return new AuthResponseDto
        {
            Token = handler.CreateToken(tokenDescriptor),
            Username = user.Username,
            ExpiresAt = expiresAt
        };
    }
}