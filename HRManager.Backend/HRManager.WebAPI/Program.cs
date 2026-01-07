using FluentValidation;
using FluentValidation.AspNetCore;
using HRManager.Application.Interfaces;
using HRManager.WebAPI.Data;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.Middlewares;
using HRManager.WebAPI.Models;
using HRManager.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings");


// 1. Adicionar o serviço CORS
const string AllowAngularOrigin = "_allowAngularOrigin";
// Lê os URLs do ficheiro de configuração
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowAngularOrigin,
                      policy =>
                      {
                          policy.WithOrigins(allowedOrigins) // Usa a lista dinâmica
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                      });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddControllers();

// 2. Modifique esta linha
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Esta é a correção: Ignora os ciclos de referência ao converter para JSON
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    // 2. ADICIONAR ESTA LINHA: Converte Enums de/para Texto (ex: "Ferias" <-> 0)
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDbContext<HRManagerDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreConnection"),
        npgsqlOptions =>
        {
            // Otimizações específicas para PostgreSQL, se necessário
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        });
});

// Registo dos Serviços de Identity (Necessário para o UserManager funcionar)
builder.Services.AddIdentityCore<User>(options =>
{
    // Configurações opcionais de password
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<Role>() // Importante para gerir os perfis (GestorRH, Colaborador)
.AddEntityFrameworkStores<HRManagerDbContext>() // Liga o Identity à tua BD
.AddDefaultTokenProviders();
// -------------------------------------

// Adicionar o serviço de acesso ao contexto HTTP
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = jwtSettings["Key"];
    if (string.IsNullOrWhiteSpace(jwtKey))
    {
        throw new InvalidOperationException("A chave JWT ('JwtSettings:Key') não está configurada.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],

        // A chave de segurança
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Adicionar o TenantService como Scoped (para cada pedido)
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAusenciaService, AusenciaService>();
builder.Services.AddScoped<IColaboradorService, ColaboradorService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInstituicaoService, InstituicaoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();
builder.Services.AddScoped<IDeclaracaoService, DeclaracaoService>();
builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddValidatorsFromAssemblyContaining<CriarColaboradorValidator>(); // Regista todos os validadores
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // Regista todos os validadores automaticamente

var app = builder.Build();

// --- BLOCO DE SEED OBRIGATÓRIO ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 1. Obter os serviços necessários
        var context = services.GetRequiredService<HRManagerDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<Role>>();

        // 2. Aplicar migrações pendentes automaticamente
        context.Database.Migrate();

        // 3. Executar o Seed (agora com await)
        await DbSeeder.Seed(context, userManager, roleManager);
        Console.WriteLine("✅ Seed da Base de Dados executado com sucesso!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao popular a base de dados (Seeding).");
        Console.WriteLine($"❌ Erro ao executar o Seed: {ex.Message}");
    }
}

//app.Use(async (context, next) =>
//{
//    Console.WriteLine($"Request: {context.Request.Path}");
//    await next();
//});
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Path}");
    Console.WriteLine($"User authenticated: {context.User?.Identity?.IsAuthenticated}");

    if (context.User?.Identity?.IsAuthenticated == true)
    {
        var tenantClaim = context.User.FindFirst("tenantId");
        Console.WriteLine($"TenantId in token: {tenantClaim?.Value}");

        var roles = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);
        Console.WriteLine($"Roles: {string.Join(", ", roles)}");
    }

    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors(AllowAngularOrigin);
app.UseStaticFiles(); // Permite aceder à pasta wwwroot
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();