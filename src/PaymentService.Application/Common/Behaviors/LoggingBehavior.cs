using MediatR;
using Microsoft.Extensions.Logging;

namespace PaymentService.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior que registra los comandos/queries ejecutados.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Iniciando solicitud {RequestName}", requestName);

        try
        {
            var result = await next();
            _logger.LogInformation("Solicitud completada {RequestName}", requestName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Solicitud falló {RequestName}: {Message}", requestName, ex.Message);
            throw;
        }
    }
}
