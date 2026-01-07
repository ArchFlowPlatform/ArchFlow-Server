using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using agileTrackerServer.Data;
using agileTrackerServer.Infrastructure.Email;
using agileTrackerServer.Middlewares;
using agileTrackerServer.Models.ViewModels;
using agileTrackerServer.Repositories.Implementations;
using agileTrackerServer.Repositories.Interfaces;
using agileTrackerServer.Services;
using agileTrackerServer.Utils;
using agileTrackerServer.Utils.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 🔐 SECURITY SETTINGS
// ============================================================================

builder.Services.Configure<SecuritySettings>(
    builder.Configuration.GetSection("Security")
);

var jwtKey = Encoding.UTF8.GetBytes(
    builder.Configuration["Security:JwtSecret"]!
);

// ============================================================================
// ⚙️ ASP.NET CORE
// ============================================================================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        );
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // MANTÉM a validação automática de ModelState
    // ❌ NÃO usar SuppressModelStateInvalidFilter = true

    // Evita respostas automáticas 404/405 sem body
    options.SuppressMapClientErrors = true;

    // Customiza a resposta de validação para seu contrato
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .SelectMany(x => x.Value!.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return new BadRequestObjectResult(
            ResultViewModel.Fail("Erro de validação", errors)
        );
    };
});


// ============================================================================
// 🔑 AUTHENTICATION (JWT via HttpOnly Cookie)
// ============================================================================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ClockSkew = TimeSpan.Zero
        };

        // 🔑 Lê o JWT do COOKIE HttpOnly
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(
                    "token",
                    out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

// ============================================================================
// 🔐 AUTHORIZATION (Roles por Type)
// ============================================================================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlusOnly", policy =>
        policy.RequireClaim("Type", "Plus", "Admin"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("Type", "Admin"));
});

// ============================================================================
// 🧩 SWAGGER
// ============================================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new()
    {
        Title = "AgileTracker Server API",
        Version = "v1"
    });
});

// ============================================================================
// 🗄️ DATABASE (PostgreSQL)
// ============================================================================

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=agiletracker_dev;Username=postgres;Password=root";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ============================================================================
// 🧩 DEPENDENCY INJECTION
// ============================================================================

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ISprintRepository, SprintRepository>();
builder.Services.AddScoped<IProjectInviteRepository, ProjectInviteRepository>();

// Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<SprintService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ProjectAuthorizationService>();
builder.Services.AddScoped<ProjectRoleAuthorizationFilter>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();


builder.Services.AddControllers(options =>
{
    options.Filters.Add<ProjectRoleAuthorizationFilter>();
});

// Utils
builder.Services.AddScoped<PasswordHasher>();

// ============================================================================
// 🌐 CORS
// ============================================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ============================================================================
// 🚀 PIPELINE
// ============================================================================

var app = builder.Build();

// ⚠️ Middleware global PRIMEIRO
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevelopmentCors");
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============================================================================
// 🗃️ MIGRATIONS
// ============================================================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();
