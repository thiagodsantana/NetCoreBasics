using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetCoreBasics;
using NetCoreBasics.Class;
using NetCoreBasics.Configurations;
using NetCoreBasics.Interfaces;
using NetCoreBasics.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Configurations
/*
 * Configurations
    - Adicionando configurações ao container DI
 */

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// 🔹 Obtendo valores diretamente da configuração para autenticação JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
#endregion

#region Configuração de Autenticação JWT
// 🔹 Configuração de Autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? "default-key"))
        };
    });

#endregion

#region Configuração do Swagger
// 🔹 Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region Injeção de dependência
/* Injeção de dependência
    - Singleton: Criado uma vez e usado globalmente.
    - Scoped: Criado por requisição e compartilhado dentro dela.
    - Transient: Criado toda vez que for solicitado.
 */
builder.Services.AddSingleton<ISingletonService, SingletonService>();
builder.Services.AddScoped<IScopedService, ScopedService>();
builder.Services.AddTransient<ITransientService, TransientService>();
#endregion

var app = builder.Build();

// 🔹 Ativando o Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region Middleware
app.UseAuthentication();

app.UseMiddleware<CustomMiddleware>();
#endregion

#region Endpoints de configuração 
app.MapGet("/config", (IOptions<AppSettings> options) =>
{
    var config = options.Value;
    return new
    {
        Aplicacao = config.ApplicationName,
        Versao = config.Version,
        FuncionalidadeX = config.EnableFeatureX ? "Ativada" : "Desativada"
    };
});

app.MapGet("/config-live", (IOptionsMonitor<AppSettings> optionsMonitor) =>
{
    var config = optionsMonitor.CurrentValue;
    return new
    {
        Aplicacao = config.ApplicationName,
        Versao = config.Version,
        FuncionalidadeX = config.EnableFeatureX ? "Ativada" : "Desativada"
    };
});
#endregion

#region .NET 6
/* .NET 6 
    - Minimal API básica com MapGet para criar endpoints.
    - Código reduzido e mais performático.
    - Usa um serviço Singleton para mostrar o mesmo ID sempre.
 */
app.MapPost("/Singleton", (ISingletonService singleton) =>
    $"Hello, World! Singleton ID: {singleton.GetId()}");
#endregion

#region .NET 7
/* .NET 7
    - Introdução de Endpoint Filters (AddEndpointFilter).
    - Permite validar a entrada antes da execução do endpoint. 
    - Usa um serviço Scoped, gerando um ID por requisição.
 */
app.MapPost("/Scoped", (User user, IScopedService scoped) =>
    Results.Ok($"Usuário: {user.Name}, Scoped ID: {scoped.GetId()}"))
   .AddEndpointFilter(async (context, next) =>
   {
       var user = context.GetArgument<User>(0);
       if (string.IsNullOrWhiteSpace(user.Name))
           return Results.BadRequest("Nome é obrigatório.");
       return await next(context);
   });

app.MapPost("/Transient", (User user, ITransientService transient) =>
    Results.Ok($"Usuário: {user.Name}, Transient ID: {transient.GetId()}"))
   .AddEndpointFilter(async (context, next) =>
   {
       var user = context.GetArgument<User>(0);
       if (string.IsNullOrWhiteSpace(user.Name))
           return Results.BadRequest("Nome é obrigatório.");
       return await next(context);
   });
#endregion

#region .NET 8

/* .NET 8
    - Uso de Route Groups para melhor organização.
    - TypedResults para retornos tipados e mais seguros.
    - Autenticação JWT para segurança e controle de acesso.
    - GET /users/{id} → Usa um serviço Transient, gerando um novo ID toda vez.
    - POST /users → Demonstra o uso de Scoped e Transient juntos.
 */
var userRoutes = app.MapGroup("/users")
                    .RequireAuthorization(); // Exige autenticação

userRoutes.MapGet("/{id:int}", (int id, ITransientService transient) =>
    TypedResults.Ok(new { Id = id, Name = $"User {id}", TransientID = transient.GetId() }));

userRoutes.MapPost("/", (User user, IScopedService scoped, ITransientService transient) =>
    Results.Created($"/users/{user.Id}", new
    {
        User = user,
        ScopedID = scoped.GetId(),
        TransientID = transient.GetId()
    }));
#endregion

app.Run();
