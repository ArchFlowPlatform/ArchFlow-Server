using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using archFlowServer.Data;
using archFlowServer.Infrastructure.Email;
using archFlowServer.Middlewares;
using archFlowServer.Models.ViewModels;
using archFlowServer.Repositories.Implementations;
using archFlowServer.Repositories.Interfaces;
using archFlowServer.Services;
using archFlowServer.Utils;
using archFlowServer.Utils.Authorization;
using archFlowServer.Utils.Authorization.Handlers;
using archFlowServer.Utils.Authorization.Requirements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// ðŸ” SECURITY SETTINGS
// ============================================================================

builder.Services.Configure<SecuritySettings>(
    builder.Configuration.GetSection("Security")
);

var jwtKey = Encoding.UTF8.GetBytes(
    builder.Configuration["Security:JwtSecret"]!
);

// ============================================================================
// âš™ï¸ ASP.NET CORE
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
    // MANTÃ‰M a validaÃ§Ã£o automÃ¡tica de ModelState
    // âŒ NÃƒO usar SuppressModelStateInvalidFilter = true

    // Evita respostas automÃ¡ticas 404/405 sem body
    options.SuppressMapClientErrors = true;

    // Customiza a resposta de validaÃ§Ã£o para seu contrato
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .SelectMany(x => x.Value!.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return new BadRequestObjectResult(
            ResultViewModel.Fail("Erro de validaÃ§Ã£o", errors)
        );
    };
});


// ============================================================================
// ðŸ”‘ AUTHENTICATION (JWT via HttpOnly Cookie)
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

        // ðŸ”‘ LÃª o JWT do COOKIE HttpOnly
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
// ðŸ” AUTHORIZATION (Roles por Type)
// ============================================================================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlusOnly", policy =>
        policy.RequireClaim("Type", "Plus", "Admin"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("Type", "Admin"));
});

// ============================================================================
// ðŸ§© SWAGGER
// ============================================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new()
    {
        Title = "ArchFlow Server API",
        Version = "v1"
    });
});

// ============================================================================
// ðŸ—„ï¸ DATABASE (PostgreSQL)
// ============================================================================

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=archflow_dev;Username=postgres;Password=root";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ============================================================================
// ðŸ§© DEPENDENCY INJECTION
// ============================================================================

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProductBacklogRepository, ProductBacklogRepository>();
builder.Services.AddScoped<IEpicRepository, EpicRepository>();
builder.Services.AddScoped<IUserStoryRepository,  UserStoryRepository>();
builder.Services.AddScoped<ISprintRepository, SprintRepository>();
builder.Services.AddScoped<IProjectInviteRepository, ProjectInviteRepository>();

// Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<BacklogService>();
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
builder.Services.AddHttpContextAccessor();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewProject",
        policy => policy.Requirements.Add(new CanViewProjectRequirement()));

    options.AddPolicy("CanArchiveProject",
        policy => policy.Requirements.Add(new CanArchiveProjectRequirement()));

    options.AddPolicy("CanInviteMembers",
        policy => policy.Requirements.Add(new CanInviteMembersRequirement()));
    
    options.AddPolicy(
        "CanManageMembers",
        policy => policy.Requirements.Add(new CanManageMembersRequirement())
    );
});

// Handlers
builder.Services.AddScoped<IAuthorizationHandler, CanViewProjectHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CanArchiveProjectHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CanInviteMembersHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CanManageMembersHandler>();
// ============================================================================
// ðŸŒ CORS
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
// ðŸš€ PIPELINE
// ============================================================================

var app = builder.Build();

// âš ï¸ Middleware global PRIMEIRO
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
// ðŸ—ƒï¸ MIGRATIONS
// ============================================================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();

