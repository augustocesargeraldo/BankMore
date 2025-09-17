using BankMore.ContaCorrente.Application.QueryHandlers.ContaCorrente;
using BankMore.ContaCorrente.Application.QueryHandlers.SaldoContaCorrente;
using BankMore.ContaCorrente.Application.Repositories;
using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Application.UseCases.CriarContaCorrente;
using BankMore.ContaCorrente.Application.UseCases.CriarSessao;
using BankMore.ContaCorrente.Application.UseCases.InativarContaCorrente;
using BankMore.ContaCorrente.Application.UseCases.MovimentacaoContaCorrente;
using BankMore.ContaCorrente.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// configurações
var configuration = builder.Configuration;

// Controllers + Swagger
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    // Desabilita o comportamento padrão de 400
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BankMore ContaCorrente API", Version = "v1" });

    // Configuração de JWT Bearer
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT assim: Bearer {seu token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
var jwtSection = configuration.GetSection("Jwt");
var secret = jwtSection.GetValue<string>("Secret");
if (string.IsNullOrWhiteSpace(secret))
{
    throw new InvalidOperationException("JWT Secret não configurado!");
}
var key = Encoding.ASCII.GetBytes(secret);

var expirationMinutes = jwtSection.GetValue<int?>("ExpirationMinutes");
builder.Services.AddScoped<IJwtService>(_ => new JwtService(secret, expirationMinutes));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    };
});

// DB Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection String não configurada!");
}
builder.Services.AddScoped<System.Data.IDbConnection>(sp =>
{
    return new SqliteConnection(connectionString);
});

builder.Services.AddScoped<IIdempotenciaService, IdempotenciaService>();

builder.Services.AddScoped<ICriarContaCorrenteUseCase, CriarContaCorrenteUseCase>();
builder.Services.AddScoped<ICriarSessaoUseCase, CriarSessaoUseCase>();
builder.Services.AddScoped<IInativarContaCorrenteUseCase, InativarContaCorrenteUseCase>();
builder.Services.AddScoped<IMovimentacaoContaCorrenteUseCase, MovimentacaoContaCorrenteUseCase>();

builder.Services.AddScoped<IObterContaCorrentePorIdQueryHandler, ObterContaCorrentePorIdQueryHandler>();
builder.Services.AddScoped<IObterContaCorrentePorNumeroQueryHandler, ObterContaCorrentePorNumeroQueryHandler>();
builder.Services.AddScoped<IObterSaldoContaCorrenteQueryHandler, ObterSaldoContaCorrenteQueryHandler>();

builder.Services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();
builder.Services.AddScoped<IMovimentoRepository, MovimentoRepository>();
builder.Services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
