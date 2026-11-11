using PaymentService.Application;
using PaymentService.Infrastructure;
using PaymentService.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════════════════
// FASE 1: Registro de servicios
// ═══════════════════════════════════════════════════════════════════════════════

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PaymentService API",
        Version = "v1",
        Description = "Simulador de pasarela de pagos. No procesa pagos reales."
    });
});

// Application (MediatR + behaviors + validators)
builder.Services.AddApplication();

// Infrastructure
builder.Services.AddInfrastructure();

// Exception handler (IExceptionHandler — se activa con app.UseExceptionHandler() más abajo)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// ═══════════════════════════════════════════════════════════════════════════════
// FASE 2: Pipeline de middleware
//
// SIN AUTENTICACIÓN (decisión deliberada): PaymentService es un servicio interno que solo
// consume MiEcommerce desde la red interna (server-to-server). No lo llama ningún usuario final,
// así que no hay un usuario para propagar: el JWT de MiEcommerce identifica clientes, no servicios.
// En producción se protegería a nivel de red (no exponerlo públicamente) o con autenticación
// servicio-a-servicio (mTLS, client credentials), no reenviando el token del usuario.
// ═══════════════════════════════════════════════════════════════════════════════

app.UseExceptionHandler(); // ← PRIMERO: captura toda excepción de la cadena

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentService API v1");
    });
}

app.MapControllers();

app.Run();
