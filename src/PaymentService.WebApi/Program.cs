using FluentValidation;
using MediatR;
using PaymentService.Application.Common.Behaviors;
using PaymentService.Application.Features.Payments.Commands.ProcessPayment;
using PaymentService.Infrastructure;

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

// MediatR + Behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<ProcessPaymentCommand>();
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

// FluentValidation — registra todos los validators del ensamblado Application
builder.Services.AddValidatorsFromAssemblyContaining<ProcessPaymentCommand>();

// Infrastructure
builder.Services.AddInfrastructure();

var app = builder.Build();

// ═══════════════════════════════════════════════════════════════════════════════
// FASE 2: Pipeline de middleware
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
