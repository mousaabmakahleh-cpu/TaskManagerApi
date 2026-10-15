using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Models;

namespace TaskManagerApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // كل DbSet = جدول
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }

    // إعدادات إضافية للجداول (اسمها Fluent API)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===== Users =====
        modelBuilder.Entity<User>(entity =>
        {
            // ما في اثنين بنفس الـ Username
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(50);
        });

        // ===== Projects =====
        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(100);

            // العلاقة: المشروع إله مستخدم واحد، والمستخدم إله مشاريع كثير
            // ولو انحذف المستخدم، بتنحذف مشاريعه معه (Cascade)
            // المشروع اله مستخدم واحد 
            //المستخدم اله اكثر من مشروع
            // userid => foreginkey 
            // واذا انحذف المستخدم بتنحذف المشاريع كامله
            entity.HasOne(p => p.User)
                  .WithMany(u => u.Projects)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== Tasks =====
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(t => t.Title).HasMaxLength(200);

            // خزّن الـ Enum كنص ("Done") بدل رقم (2)، عشان يكون مقروء بالـ pgAdmin
            entity.Property(t => t.Status).HasConversion<string>();
            entity.Property(t => t.Priority).HasConversion<string>();

            // العلاقة: المهمة إلها مشروع واحد، والمشروع إله مهام كثير
            // ولو انحذف المشروع، بتنحذف مهامه معه
            entity.HasOne(t => t.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}