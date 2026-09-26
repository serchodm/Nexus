using FastEndpoints;
using FastEndpoints.Swagger;
using Nexus.Application;
using Nexus.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 0. Soporte para codificaciones heredadas de banca española (Windows-1252/ISO-8859-1)
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

// 1. Inyección de Dependencias por Capas (Clean Architecture)
builder.Services.AddMemoryCache();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. FastEndpoints y Documentación OpenAPI
builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "Nexus API";
            s.Version = "v1";
            s.Description = "API de alto rendimiento con FastEndpoints, Clean Architecture y PostgreSQL";
        };
    });

// 3. Política de CORS para Frontend (React 19 + Vite)
const string corsPolicyName = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins(
            builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Requerido para la Cookie HttpOnly del Refresh Token
    });
});

var app = builder.Build();

// 4. Pipeline HTTP
app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.Run();

// Para tests de integración futuros
public partial class Program { }
