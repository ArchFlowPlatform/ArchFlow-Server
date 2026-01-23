# 🚀 ArchFlow Server

> Backend robusto e escalável para gestão de projetos ágeis, construído com .NET 9 e seguindo arquitetura em camadas.

---

## 📋 Índice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Arquitetura e Estrutura](#-arquitetura-e-estrutura)
  - [Visão Geral](#visão-geral)
  - [Controllers](#1-controllers)
  - [Services](#2-services)
  - [Repositories](#3-repositories)
  - [Domain (Entidades)](#4-domain-entidades)
  - [DTOs](#5-dtos)
  - [Middlewares](#6-middlewares)
  - [ViewModels](#7-viewmodels)
- [Entity Framework Core](#-entity-framework-core)
- [Tratamento de Erros](#-tratamento-de-erros)
- [Autenticação e Autorização](#-autenticação-e-autorização)
- [Tecnologias Utilizadas](#-tecnologias-utilizadas)
- [Como Rodar o Projeto](#-como-rodar-o-projeto)
- [Endpoints da API](#-endpoints-da-api)

---

## 📖 Sobre o Projeto

O **ArchFlow Server** é uma API RESTful desenvolvida em **.NET 9** que oferece recursos completos para gestão de projetos ágeis, integrando metodologias como **Scrum** e **Kanban**. 

O sistema permite:
- Gerenciamento de projetos, sprints e backlogs
- Controle de tarefas com quadros Kanban
- Dashboard com métricas de desempenho
- Documentação e diagramas versionados
- Sistema de autenticação via JWT com cookies HttpOnly

**Diferenciais técnicos:**
- Arquitetura limpa baseada em camadas
- Separação clara de responsabilidades
- Validações em múltiplas camadas
- Tratamento global de exceções
- Entity Framework Core com PostgreSQL

---

## 🏗️ Arquitetura e Estrutura

### Visão Geral

O projeto segue uma arquitetura em camadas inspirada em **Domain-Driven Design**, onde cada camada tem uma responsabilidade bem definida:

```
ArchFlow-Server/
│
├── Controllers/          # Endpoints da API (Presentation Layer)
├── Services/             # Lógica de negócio (Application Layer)
├── Repositories/         # Acesso a dados (Infrastructure Layer)
│   ├── Interfaces/
│   └── Implementations/
├── Models/
│   ├── Entities/         # Entidades de domínio (Domain Layer)
│   ├── Dtos/             # Data Transfer Objects
│   ├── ViewModels/       # Modelos de resposta da API
│   ├── Enums/            # Enumeradores
│   └── Exceptions/       # Exceções customizadas
├── Middlewares/          # Interceptadores de requisições
├── Data/                 # Contexto do EF Core
└── Utils/                # Utilitários e extensões
```

---

### 1. **Controllers**

**Localização:** `Controllers/`

**Responsabilidade:** Receber requisições HTTP, validar dados de entrada, chamar os serviços apropriados e retornar respostas padronizadas.

Os controllers **não contêm lógica de negócio**. Eles apenas orquestram a comunicação entre a camada de apresentação e a camada de aplicação.

#### Exemplo: `ProjectsController.cs`

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _service;

    public ProjectsController(ProjectService service)
    {
        _service = service;
    }

    // GET api/projects
    [HttpGet]
    [SwaggerOperation(Summary = "Lista todos os projetos ativos do usuário logado.")]
    [ProducesResponseType(typeof(ResultViewModel<IEnumerable<ProjectResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId(); // Extension method para extrair ID do JWT

        var projects = await _service.GetAllAsync(userId);

        return Ok(
            ResultViewModel<IEnumerable<ProjectResponseDto>>.Ok(
                "Projetos carregados com sucesso.",
                projects
            )
        );
    }

    // POST api/projects
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um novo projeto.")]
    [ProducesResponseType(typeof(ResultViewModel<ProjectResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto request)
    {
        var userId = User.GetUserId();

        var project = await _service.CreateAsync(request, userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = project.Id },
            ResultViewModel<ProjectResponseDto>.Ok(
                "Projeto criado com sucesso.",
                project
            )
        );
    }
}
```

**Por que nesta pasta?**
- Controllers representam a **camada de apresentação** (Presentation Layer)
- Lidam exclusivamente com HTTP: requisições, respostas, status codes
- Delegam toda a lógica de negócio para os **Services**

---

### 2. **Services**

**Localização:** `Services/`

**Responsabilidade:** Implementar a lógica de negócio da aplicação, coordenar operações entre repositórios e aplicar regras de domínio.

Os services são o **coração da aplicação**. Eles:
- Aplicam regras de negócio
- Coordenam múltiplas operações
- Fazem a ponte entre Controllers e Repositories
- Transformam entidades em DTOs

#### Exemplo: `ProjectService.cs`

```csharp
public class ProjectService
{
    private readonly IProjectRepository _repository;

    public ProjectService(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, Guid ownerId)
    {
        // 1. Criação da entidade de domínio (validações internas)
        var project = new Project(dto.Name, dto.Description, ownerId);

        // 2. Persistência
        await _repository.AddAsync(project);
        await _repository.SaveChangesAsync();

        // 3. Conversão para DTO de resposta
        return MapToDto(project);
    }

    public async Task<ProjectResponseDto> UpdateAsync(
        Guid projectId,
        UpdateProjectDto dto,
        Guid ownerId)
    {
        // 1. Busca e validação de existência
        var project = await _repository.GetByIdAsync(projectId, ownerId)
            ?? throw new DomainException("Projeto não encontrado.");

        // 2. Atualização através do método da entidade
        project.UpdateDetails(dto.Name, dto.Description);

        // 3. Persistência
        await _repository.SaveChangesAsync();

        return MapToDto(project);
    }

    private static ProjectResponseDto MapToDto(Project project)
    {
        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            OwnerName = project.Owner?.Name ?? string.Empty,
            Status = project.Status,
            CreatedAt = project.CreatedAt
        };
    }
}
```

**Por que nesta pasta?**
- Services representam a **camada de aplicação** (Application Layer)
- Concentram a lógica de negócio e orquestração
- Garantem que as regras de domínio sejam aplicadas corretamente
- Isolam controllers de detalhes de persistência

---

### 3. **Repositories**

**Localização:** `Repositories/Interfaces/` e `Repositories/Implementations/`

**Responsabilidade:** Abstrair o acesso a dados, encapsular queries e operações de persistência.

Os repositories seguem o **Repository Pattern**, com interfaces para desacoplamento e implementações concretas usando Entity Framework Core.

#### Interface: `IProjectRepository.cs`

```csharp
public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllAsync(Guid ownerId);
    Task<Project?> GetByIdAsync(Guid id, Guid OwnerId);
    Task AddAsync(Project project);
    Task SaveChangesAsync();
}
```

#### Implementação: `ProjectRepository.cs`

```csharp
public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;
    
    public ProjectRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Project>> GetAllAsync(Guid ownerId) =>
        await _context.Projects
            .Include(p => p.Owner) // Eager loading da navegação
            .Where(p  => 
                p.OwnerId == ownerId && 
                p.Status == ProjectStatus.Active) // Filtra apenas ativos
            .ToListAsync();

    public async Task<Project?> GetByIdAsync(Guid id, Guid ownerId) =>
        await _context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p =>
                p.Id == id &&
                p.OwnerId == ownerId &&
                p.Status == ProjectStatus.Active
            );
    
    public async Task AddAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
    }

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
```

**Por que esta separação?**
- **Interfaces:** Definem contratos, permitindo inversão de dependência (SOLID)
- **Implementations:** Contêm detalhes de acesso a dados (EF Core)
- Facilita testes unitários (mock das interfaces)
- Permite trocar a tecnologia de persistência sem impactar services

---

### 4. **Domain (Entidades)**

**Localização:** `Models/Entities/`

**Responsabilidade:** Representar conceitos do negócio com comportamento e regras de validação encapsulados.

As entidades são **ricas em comportamento** (não são apenas bags de propriedades). Elas:
- Validam seus próprios dados
- Expõem métodos de negócio
- Garantem consistência interna

#### Exemplo: `Project.cs`

```csharp
public class Project
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProjectStatus Status { get; private set; } = ProjectStatus.Active;
    public Guid OwnerId { get; private set; }

    public User? Owner { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private Project() { } // Construtor privado para EF Core

    // Construtor público com validações
    public Project(string name, string? description, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do projeto é obrigatório.");

        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId inválido.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Status = ProjectStatus.Active;
        OwnerId = ownerId;
        CreatedAt = DateTime.UtcNow;
    }

    // Método de negócio com validação
    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do projeto é obrigatório.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    // Método de negócio
    public void Archive()
    {
        Status = ProjectStatus.Archived;
    }
}
```

**Por que nesta pasta?**
- Entidades são o **núcleo do domínio** (Domain Layer)
- Concentram regras de negócio críticas
- Garantem que o objeto nunca fique em estado inválido
- Setters privados protegem a integridade dos dados

---

### 5. **DTOs**

**Localização:** `Models/Dtos/`

**Responsabilidade:** Transferir dados entre camadas sem expor detalhes internos das entidades.

Os DTOs (Data Transfer Objects):
- Definem contratos de entrada/saída da API
- Contêm validações de dados (Data Annotations)
- Evitam over-posting e under-posting

#### Exemplo: `CreateProjectDto.cs`

```csharp
[SwaggerSchema(Description = "DTO para criação de um projeto.")]
public class CreateProjectDto
{
    [SwaggerSchema("Nome do projeto.")]
    [Required(ErrorMessage = "Nome é obrigatório.")]
    [MinLength(3, ErrorMessage = "Nome deve ter no mínimo 3 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [SwaggerSchema("Descrição do projeto.")]
    public string Description { get; set; } = string.Empty;
}
```

**Por que nesta pasta?**
- DTOs são contratos de comunicação (Application Layer)
- Isolam a API de mudanças no domínio
- Validações via Data Annotations são interceptadas automaticamente

---

### 6. **Middlewares**

**Localização:** `Middlewares/`

**Responsabilidade:** Interceptar requisições HTTP para aplicar comportamentos transversais (cross-cutting concerns).

Middlewares processam **toda requisição** antes de chegar aos controllers.

#### Exemplo: `GlobalExceptionMiddleware.cs`

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context); // Passa para o próximo middleware/controller
        }
        catch (DomainException ex)
        {
            await HandleDomainExceptionAsync(context, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleUnauthorizedExceptionAsync(context, ex);
        }
        catch (Exception)
        {
            await HandleInternalExceptionAsync(context);
        }
    }

    // Trata exceções de domínio como 404
    private static Task HandleDomainExceptionAsync(
        HttpContext context,
        DomainException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";

        var result = ResultViewModel.Fail(ex.Message);

        return context.Response.WriteAsJsonAsync(result);
    }

    // Outros handlers...
}
```

**Registro no `Program.cs`:**

```csharp
// ⚠️ Middleware global PRIMEIRO (antes de tudo)
app.UseMiddleware<GlobalExceptionMiddleware>();
```

**Por que nesta pasta?**
- Middlewares implementam **cross-cutting concerns** (logging, erro handling, etc.)
- Centralizam tratamento de erros, evitando try-catch em todos os controllers
- São executados na ordem de registro no pipeline

---

### 7. **ViewModels**

**Localização:** `Models/ViewModels/`

**Responsabilidade:** Padronizar respostas da API com estrutura consistente.

#### Exemplo: `ResultViewModel.cs`

```csharp
public class ResultViewModel<T> : ResultViewModel
{
    public new T Data
    {
        get => (T)base.Data!;
        set => base.Data = value!;
    }

    public ResultViewModel(
        string message,
        bool success = true,
        T? data = default,
        List<string>? errors = null)
        : base(message, success, data ?? default(T)!, errors)
    {
    }

    public static ResultViewModel<T> Ok(string message, T data)
        => new(message, true, data);

    public static ResultViewModel<T> Fail(string message, List<string>? errors)
        => new(message, false, default(T)!, errors);
}
```

**Resposta padrão da API:**

```json
{
  "message": "Projeto criado com sucesso.",
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Projeto Alpha",
    "description": "Descrição do projeto"
  },
  "errors": null
}
```

**Por que nesta pasta?**
- ViewModels padronizam contratos de saída
- Facilitam tratamento no frontend
- Melhoram a experiência do desenvolvedor consumidor da API

---

## 🗄️ Entity Framework Core

O projeto utiliza **EF Core** com **PostgreSQL** seguindo **Code First approach**.

### Configuração no `Program.cs`

```csharp
// Desabilita conversão automática de timestamps
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=archflow_dev;Username=postgres;Password=root";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
```

### Migrations Automáticas

```csharp
// Aplica migrations automaticamente na inicialização
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}
```

### Boas Práticas Aplicadas

1. **Eager Loading:** Uso de `.Include()` para carregar navegações relacionadas
2. **Async/Await:** Todas as operações de I/O são assíncronas
3. **Queries Filtradas:** Repositórios encapsulam queries complexas
4. **Unit of Work:** `SaveChangesAsync()` gerencia transações automaticamente

---

## ⚠️ Tratamento de Erros

O sistema possui **tratamento de erros em 3 camadas**:

### 1. **Validação de DTOs (Controllers)**

```csharp
// Configurado em Program.cs
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
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
```

### 2. **Validação de Domínio (Entidades)**

```csharp
// Na entidade Project
public Project(string name, string? description, Guid ownerId)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new DomainException("Nome do projeto é obrigatório.");
    // ...
}
```

### 3. **Tratamento Global (Middleware)**

```csharp
// GlobalExceptionMiddleware captura todas as exceções
catch (DomainException ex)
{
    // Retorna 404 com mensagem amigável
    context.Response.StatusCode = StatusCodes.Status404NotFound;
    var result = ResultViewModel.Fail(ex.Message);
    return context.Response.WriteAsJsonAsync(result);
}
```

**Fluxo de Erro:**
1. Controller valida DTOs → `400 Bad Request`
2. Entidade valida regras de negócio → `DomainException`
3. Middleware captura exceção → `404 Not Found` ou `500 Internal Server Error`

---

## 🔐 Autenticação e Autorização

### JWT via HttpOnly Cookies

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Validação do token
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
                if (context.Request.Cookies.TryGetValue("token", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });
```

### Autorização por Roles

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlusOnly", policy =>
        policy.RequireClaim("Type", "Plus", "Admin"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("Type", "Admin"));
});
```

### Uso nos Controllers

```csharp
[Authorize] // Requer autenticação
[ApiController]
public class ProjectsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId(); // Extension method
        // ...
    }
}
```

---

## 🛠️ Tecnologias Utilizadas

- **.NET 9** - Framework principal
- **ASP.NET Core** - API REST
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **JWT** - Autenticação
- **Swagger/OpenAPI** - Documentação
- **Swashbuckle** - Geração de documentação interativa

---

## 🚀 Como Rodar o Projeto

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/pt-br/download/dotnet/9.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- Editor de código (VS Code, Visual Studio, Rider)

### Passo a Passo

1. **Clone o repositório**

```bash
git clone https://github.com/ArchFlowPlatform/ArchFlow-Server.git
cd ArchFlow-Server
```

2. **Configure o banco de dados**

Edite `appsettings.json` ou `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=archflow_dev;Username=postgres;Password=sua_senha"
  },
  "Security": {
    "JwtSecret": "sua-chave-secreta-super-segura-aqui"
  }
}
```

3. **Restaure as dependências**

```bash
dotnet restore
```

4. **Execute as migrations**

```bash
dotnet ef database update
```

Ou simplesmente rode o projeto (migrations automáticas configuradas):

```bash
dotnet run
```

5. **Acesse a documentação**

Abra seu navegador em: `https://localhost:5001/swagger`

---

## 📡 Endpoints da API

### Projetos

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/projects` | Lista projetos do usuário |
| GET | `/api/projects/{id}` | Busca projeto por ID |
| POST | `/api/projects` | Cria novo projeto |
| PUT | `/api/projects/{id}` | Atualiza projeto |
| POST | `/api/projects/{id}/archive` | Arquiva projeto |

### Autenticação

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/auth/register` | Registra novo usuário |
| POST | `/api/auth/login` | Realiza login |
| POST | `/api/auth/logout` | Realiza logout |

**Documentação completa:** Acesse `/swagger` quando o servidor estiver rodando.

---

## 📝 Licença

Este projeto está sob a licença MIT.

---

## 👥 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues e pull requests.

---

**Desenvolvido com 💙 usando .NET 9**
