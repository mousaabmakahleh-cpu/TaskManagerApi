using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Dtos;
using TaskManagerApi.Models;

namespace TaskManagerApi.Controllers;

[ApiController]
[Route("api/v1")]   // الروابط كاملة مكتوبة فوق كل method
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    // رقم المستخدم من التوكن
    private int GetUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    // هل هالمشروع موجود وإله؟
    private async Task<bool> UserOwnsProject(int projectId, int userId)
    {
        return await _context.Projects.AnyAsync(p => p.Id == projectId && p.UserId == userId);
    }

    // GET: api/v1/projects/5/tasks?status=Done&priority=High&search=api&page=1&pageSize=10
    [HttpGet("projects/{projectId}/tasks")]
    public async Task<ActionResult<PagedResult<TaskResponseDto>>> GetProjectTasks(
        int projectId,
        [FromQuery] TaskItemStatus? status,
        [FromQuery] TaskPriority? priority,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        // 1. المشروع لازم يكون إله
        if (!await UserOwnsProject(projectId, GetUserId()))
        {
            return NotFound(new { message = "Project not found" }); //404
        }

        // 2. مهام هالمشروع، مع الفلاتر
        IQueryable<TaskItem> query = _context.Tasks.Where(t => t.ProjectId == projectId);

        if (status != null)
        {
            query = query.Where(t => t.Status == status);
        }

        if (priority != null)
        {
            query = query.Where(t => t.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Title.ToLower().Contains(search.ToLower()));
        }

        int totalCount = await query.CountAsync();

        List<TaskResponseDto> items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                ProjectId = t.ProjectId
            })
            .ToListAsync();

        PagedResult<TaskResponseDto> result = new PagedResult<TaskResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Ok(result); //200
    }

    // POST: api/v1/projects/5/tasks
    [HttpPost("projects/{projectId}/tasks")]
    public async Task<ActionResult<TaskResponseDto>> Create(int projectId, CreateTaskDto dto)
    {
        if (!await UserOwnsProject(projectId, GetUserId()))
        {
            return NotFound(new { message = "Project not found" }); //404
        }

        TaskItem task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            ProjectId = projectId   // من الرابط
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(); // INSERT

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, ToResponse(task)); //201
    }

    // GET: api/v1/tasks/7
    [HttpGet("tasks/{id}")]
    public async Task<ActionResult<TaskResponseDto>> GetById(int id)
    {
        TaskItem? task = await FindUserTask(id);

        if (task == null)
        {
            return NotFound(); //404
        }

        return Ok(ToResponse(task)); //200
    }

    // PATCH: api/v1/tasks/7
    [HttpPatch("tasks/{id}")]
    public async Task<ActionResult<TaskResponseDto>> Update(int id, UpdateTaskDto dto)
    {
        TaskItem? task = await FindUserTask(id);

        if (task == null)
        {
            return NotFound(); //404
        }

        // بس الحقول اللي انبعتت بتتغير
        if (dto.Title != null) task.Title = dto.Title;
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.Status != null) task.Status = dto.Status.Value;
        if (dto.Priority != null) task.Priority = dto.Priority.Value;
        if (dto.DueDate != null) task.DueDate = dto.DueDate;

        await _context.SaveChangesAsync(); // UPDATE

        return Ok(ToResponse(task)); //200
    }

    // DELETE: api/v1/tasks/7
    [HttpDelete("tasks/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        TaskItem? task = await FindUserTask(id);

        if (task == null)
        {
            return NotFound(); //404
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(); // DELETE

        return NoContent(); //204
    }

    // ندور على مهمة، بشرط تكون بمشروع تابع للمستخدم
    // t.Project!.UserId: الـ EF بيعمل JOIN بين Tasks و Projects لحاله
    private async Task<TaskItem?> FindUserTask(int id)
    {
        int userId = GetUserId();

        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.Project!.UserId == userId);
    }

    // Mapping: من TaskItem لـ TaskResponseDto
    private static TaskResponseDto ToResponse(TaskItem task)
    {
        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            ProjectId = task.ProjectId
        };
    }
}