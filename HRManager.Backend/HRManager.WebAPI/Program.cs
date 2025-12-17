using FluentValidation;
using FluentValidation.AspNetCore;
using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.Middlewares;
using HRManager.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

// Adicionar o serviço de acesso ao contexto HTTP
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],

        // A chave de segurança
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
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
        var context = services.GetRequiredService<HRManagerDbContext>();
        context.Database.Migrate(); // Aplica migrações pendentes

        // Executa o seed
        HRManager.WebAPI.Data.DbSeeder.Seed(context);
        Console.WriteLine("✅ Seed da Base de Dados executado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro ao executar o Seed: {ex.Message}");
    }
}

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Path}");
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