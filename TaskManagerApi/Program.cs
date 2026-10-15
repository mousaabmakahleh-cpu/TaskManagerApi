using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskManagerApi.Data;
using TaskManagerApi.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ملف الـ OpenAPI + نضيفله إنه الـ API بيستخدم JWT (عشان زر Authorize بالـ Swagger)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // 1. نعرّف نوع الحماية: Bearer Token من نوع JWT
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste your JWT token here (without the word Bearer)"
        };

        // 2. نطبقها على كل الـ endpoints
        document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});

// ربط الـ AppDbContext بـ PostgreSQL (الرابط من الـ User Secrets)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// تسجيل الـ Error Handler العام
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// تفعيل الـ JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,            // مين أصدره؟
            ValidateAudience = true,          // لمين معمول؟
            ValidateLifetime = true,          // منتهي ولا لا؟
            ValidateIssuerSigningKey = true,  // الختم سليم؟

            // الـ Issuer والـ Audience من appsettings.json، والـ Key من الـ User Secrets
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// لازم يكون بأول الـ pipeline عشان يمسك الأخطاء من كل اللي بعده
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Task Manager API v1");
        options.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();

// الترتيب مهم: أول إشي مين إنت، وبعدها شو مسموحلك
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();