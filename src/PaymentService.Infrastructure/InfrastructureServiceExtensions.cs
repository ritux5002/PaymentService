using Microsoft.Extensions.DependencyInjection;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Middleware;
using PaymentService.Infrastructure.Repositories;

namespace PaymentService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();

        services.AddExceptionHandler<PaymentExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
