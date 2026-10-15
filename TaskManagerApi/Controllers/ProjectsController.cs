using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Dtos;
using TaskManagerApi.Models;

namespace TaskManagerApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]   // فوق الكلاس: كل الـ endpoints هون بدها توكن
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext context)
    {
        _context = context;
    }

    // نجيب رقم المستخدم من التوكن
    // User هون = المستخدم اللي عامل login (من التوكن)، مش كلاس User تبعنا
    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    // GET: api/v1/projects?page=1&pageSize=10&search=web
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectResponseDto>>> GetMyProjects(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        int userId = GetUserId();

        // أهم سطر: مشاريع هالمستخدم بس
        IQueryable<Project> query = _context.Projects.Where(p => p.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
        }

        int totalCount = await query.CountAsync();

        // Select: بنحوّل كل Project لـ ProjectResponseDto جوا الـ SQL نفسه
        List<ProjectResponseDto> items = await query
            .OrderByDescending(p => p.CreatedAt)   // الأحدث أول
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                TaskCount = p.Tasks.Count
            })
            .ToListAsync();

        PagedResult<ProjectResponseDto> result = new PagedResult<ProjectResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Ok(result); //200
    }

    // GET: api/v1/projects/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectResponseDto>> GetById(int id)
    {
        int userId = GetUserId();

        // لازم الـ Id صح، وكمان المشروع إله
        ProjectResponseDto? project = await _context.Projects
            .Where(p => p.Id == id && p.UserId == userId)
            .Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                TaskCount = p.Tasks.Count
            })
            .FirstOrDefaultAsync();

        if (project == null)
        {
            return NotFound(); //404
        }

        return Ok(project); //200
    }

    // POST: api/v1/projects
    [HttpPost]
    public async Task<ActionResult<ProjectResponseDto>> Create(CreateProjectDto dto)
    {
        Project project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            UserId = GetUserId()   // صاحب المشروع = اللي عامل login
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(); // INSERT

        ProjectResponseDto response = ToResponse(project, 0); // مشروع جديد = 0 مهام

        return CreatedAtAction(nameof(GetById), new { id = project.Id }, response); //201
    }

    // PUT: api/v1/projects/5
    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectResponseDto>> Update(int id, CreateProjectDto dto)
    {
        int userId = GetUserId();

        Project? project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (project == null)
        {
            return NotFound(); //404
        }

        project.Name = dto.Name;
        project.Description = dto.Description;

        await _context.SaveChangesAsync(); // UPDATE

        int taskCount = await _context.Tasks.CountAsync(t => t.ProjectId == id);

        return Ok(ToResponse(project, taskCount)); //200
    }

    // DELETE: api/v1/projects/5
    // ومهامه بتنحذف معه لحالها (Cascade)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = GetUserId();

        Project? project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (project == null)
        {
            return NotFound(); //404
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(); // DELETE

        return NoContent(); //204
    }

    // Mapping: من Project لـ ProjectResponseDto
    private static ProjectResponseDto ToResponse(Project project, int taskCount)
    {
        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            TaskCount = taskCount
        };
    }
}