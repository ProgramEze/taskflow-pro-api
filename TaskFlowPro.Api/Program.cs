using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Application.Services;
using TaskFlowPro.Infrastructure.Auth;
using TaskFlowPro.Infrastructure.Data;
using TaskFlowPro.Infrastructure.Repositories;
using TaskFlowPro.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskFlowPro.Api",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresá el token JWT. Ejemplo: Bearer {tu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
/*
userId: 50185452-dfe4-4120-9fce-5b172bf78169
workspaceId: 6b8a1c85-4a22-4a03-a0c4-7a676beb3dff
projectId: d27fcfc0-3d98-4c46-85e0-e99a020dc6fa
Tarea 1: a0e99ecd-efb1-4d96-9dbb-a5c1b610e148
Tarea 2: ca13e46a-58cf-4117-b0d2-28b5c192e1fc
Tarea 3: 4e501127-548a-4a37-8b5d-8534c8046a87
Tarea 4: 00a567e5-d84b-438b-ac70-f4d25d1dc560
Tarea 5: 6d4c6e33-ce68-430f-947c-10c35df6e57f
Tarea 6: 8be83290-d5aa-44cb-bcdf-f0c2b5291386

{
  "email": "eze@test.com",
  "password": "123456"
}

{
  "userId": "50185452-dfe4-4120-9fce-5b172bf78169",
  "firstName": "Ezequiel",
  "lastName": "Diaz",
  "email": "eze@test.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjUwMTg1NDUyLWRmZTQtNDEyMC05ZmNlLTViMTcyYmY3ODE2OSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImV6ZUB0ZXN0LmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJFemVxdWllbCBEaWF6IiwiZXhwIjoxNzgwMzQ4MTI5LCJpc3MiOiJUYXNrRmxvd1BybyIsImF1ZCI6IlRhc2tGbG93UHJvVXNlcnMifQ.vlWKUQmP77j9FMFSFo__mwa_P1mj1YgLR-_MtmB4tcE"
}

{
  "email": "ana@test.com",
  "password": "123456"
}

{
  "userId": "29549a54-e57f-42fc-a0a1-1a7ea251b705",
  "firstName": "Ana",
  "lastName": "Perez",
  "email": "ana@test.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjI5NTQ5YTU0LWU1N2YtNDJmYy1hMGExLTFhN2VhMjUxYjcwNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFuYUB0ZXN0LmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJBbmEgUGVyZXoiLCJleHAiOjE3ODAzNDkyNjQsImlzcyI6IlRhc2tGbG93UHJvIiwiYXVkIjoiVGFza0Zsb3dQcm9Vc2VycyJ9.SS4nqPqUxVXSSVm0oBqfBkayqGw-LD-8rar1bHp1cG0"
}

*/

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddScoped<IWorkspaceAuthorizationService, WorkspaceAuthorizationService>();

builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();

builder.Services.AddScoped<IWorkspaceMemberRepository, WorkspaceMemberRepository>();
builder.Services.AddScoped<IWorkspaceMemberService, WorkspaceMemberService>();

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key no configurada.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(jwtKey);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();