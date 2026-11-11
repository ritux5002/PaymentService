using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Common.Interfaces;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Persistence.Repositories;

namespace PaymentService.Infrastructure;

public static class DependencyInjection
{
    private const string InMemoryDatabaseName = "PaymentServiceDb";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // EF Core InMemory: los datos viven mientras dure el proceso (simulador, sin base real)
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseInMemoryDatabase(InMemoryDatabaseName));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
