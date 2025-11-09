using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HRManager.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings");


// 1. Adicionar o serviço CORS
const string AllowAngularOrigin = "_allowAngularOrigin";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowAngularOrigin,
                      policy =>
                      {
                          // **IMPORTANTE:** Mudar para o domínio de produção quando o deploy for feito.
                          // Para desenvolvimento local:
                          policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials(); // Permite cookies/cabeçalhos de autorização
                      });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddControllers();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(AllowAngularOrigin);
// Adicionar o middleware de Autenticação e Autorização
//app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();