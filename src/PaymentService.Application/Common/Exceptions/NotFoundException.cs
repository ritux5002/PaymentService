namespace PaymentService.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando una entidad no existe.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id {id} was not found")
    {
    }
}
