using agileTrackerServer.Data;
using agileTrackerServer.Services;
using agileTrackerServer.Repositories.Implementations;
using agileTrackerServer.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// ⚙️ CONFIGURAÇÃO BÁSICA DO ASP.NET CORE
// ============================================================================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // Evita resposta 400 automática em ModelState inválido (tratado manualmente)
    options.SuppressModelStateInvalidFilter = true;
});

// ============================================================================
// 🧩 SWAGGER / OPENAPI
// ============================================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "AgileTracker Server API",
        Version = "v1",
        Description = "API para gestão de backlog, sprints, kanban e modelagem de dados (Next.js frontend)",
        Contact = new()
        {
            Name = "Equipe AgileTracker",
            Email = "dev@agiletracker.io"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ============================================================================
// 🗄️ CONFIGURAÇÃO DO BANCO DE DADOS (PostgreSQL)
// ============================================================================

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("⚠️ Nenhuma connection string encontrada. Usando PostgreSQL local padrão.");
    connectionString = "Host=localhost;Port=5432;Database=agiletracker_dev;Username=postgres;Password=root";
}


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


// ============================================================================
// ⚙️ INJEÇÃO DE DEPENDÊNCIAS (SERVIÇOS E REPOSITÓRIOS)
// ============================================================================

// Repositories
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISprintRepository, SprintRepository>();

// Services
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SprintService>();

// ============================================================================
// 🌐 CORS (Substituindo Angular → Next.js)
// ============================================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    options.AddPolicy("ProductionCors", policy =>
    {
        policy.WithOrigins("https://app.agiletracker.io", "https://agiletracker.io")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ============================================================================
// 🪵 LOGGING
// ============================================================================

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Logging.SetMinimumLevel(
    builder.Environment.IsProduction() ? LogLevel.Warning : LogLevel.Information
);

// ============================================================================
// 🚀 PIPELINE DE EXECUÇÃO
// ============================================================================

var app = builder.Build();

// Desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgileTracker API v1");
        c.RoutePrefix = string.Empty;
        c.DisplayRequestDuration();
    });

    app.UseDeveloperExceptionPage();
    app.UseCors("DevelopmentCors");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
    app.UseCors("ProductionCors");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// ============================================================================
// 🗃️ MIGRAÇÃO AUTOMÁTICA DO BANCO
// ============================================================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        await context.Database.MigrateAsync();
        app.Logger.LogInformation("✅ Banco de dados migrado com sucesso");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ Erro ao aplicar migrações no banco de dados");

        if (app.Environment.IsProduction())
            throw;
    }
}

// ============================================================================
// ✅ INICIALIZAÇÃO
// ============================================================================

app.Logger.LogInformation("=== AgileTracker Server iniciado ===");
app.Logger.LogInformation("Ambiente: {Env}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Banco: {Conn}", connectionString);

if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("Swagger disponível em: /");
    app.Logger.LogInformation("Frontend Next.js: http://localhost:3000");
}

app.Run();
