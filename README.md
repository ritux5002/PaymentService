# PaymentService

Microservicio independiente que simula una pasarela de pagos. **No integra ninguna pasarela
real** (Stripe, MercadoPago, etc.): el propio servicio decide si aprueba o rechaza el pago, con
una regla local, explícita y determinística.

## Arquitectura

Clean Architecture en 4 capas, con CQRS vía MediatR y FluentValidation. Es el mismo esquema que
`MiEcommerce`, del que este servicio es totalmente independiente: tiene su propia solución, sus
puertos y su ciclo de vida.

```
PaymentService.Domain          Entities/Payment · ValueObjects/Money · Enums/PaymentStatus · Exceptions/
PaymentService.Application     Features/Payments/{ProcessPayment, GetPaymentById, GetPayments}/
                               Common/ (Behaviors, Exceptions, Interfaces) · DependencyInjection.cs
PaymentService.Infrastructure  Persistence/ (PaymentDbContext, UnitOfWork, Configurations, Repositories)
                               DependencyInjection.cs
PaymentService.WebApi          Controllers/ · Contracts/ · Middleware/GlobalExceptionHandler · Program.cs
```

La persistencia usa **EF Core con el provider InMemory** (`Microsoft.EntityFrameworkCore.InMemory` 8.0.11).
Los datos viven mientras dure el proceso: al reiniciar se pierde el historial, algo aceptable
para un simulador.

## Regla de aprobación

Definida en [`Payment.Create(orderId, Money)`](src/PaymentService.Domain/Entities/Payment.cs):

> Se **aprueba** el pago si el monto es **estrictamente menor a $100.000**.
> Se **rechaza** si es **igual o mayor a $100.000**.

```csharp
public const decimal ApprovalLimit = 100_000m;
var status = amount.Amount < ApprovalLimit ? PaymentStatus.Approved : PaymentStatus.Rejected;
```

- Cada pago genera un `transactionId` con formato `TX-XXXXXXXX`.
- Un monto menor o igual a cero se rechaza en dos niveles: el validator responde **400** y,
  si se llega a construir el value object `Money`, lanza `InvalidPaymentAmountException` (**422**).

## Idempotencia

Antes de crear un pago, `ProcessPaymentCommandHandler` busca si ya hay uno para ese `OrderId`.
Si existe, **devuelve el mismo pago**, con el mismo status y `transactionId`, sin crear otro.

Esto permite que MiEcommerce reintente un `confirm` sin riesgo de cobrar dos veces, por ejemplo
cuando el primer intento se cobró pero la respuesta se perdió por timeout.

## Endpoints

```
POST /api/payments/process        { "orderId": "guid", "amount": 45000 }  →  { "status", "transactionId" }
GET  /api/payments                lista de pagos
GET  /api/payments/{id}           un pago (404 si no existe)
```

| Caso | Respuesta |
|---|---|
| `amount = 45000` | `200 { "status": "Approved", "transactionId": "TX-9F3C1A2B" }` |
| `amount = 150000` | `200 { "status": "Rejected", "transactionId": "TX-4D8E7F01" }` |
| `orderId` vacío o `amount <= 0` | `400` ProblemDetails con `errors` |

Hay ejemplos listos en [`PaymentService.WebApi.http`](src/PaymentService.WebApi/PaymentService.WebApi.http).

## Puerto y ejecución

| Perfil | URL |
|---|---|
| https | `https://localhost:7200` (el que usa MiEcommerce) |
| http | `http://localhost:5200` |

```bash
dotnet run --project src/PaymentService.WebApi --launch-profile https
```

Swagger queda en `https://localhost:7200/swagger`. Para compilar: `dotnet build PaymentService.sln`.

## Sin autenticación (servicio interno)

PaymentService solo lo llama MiEcommerce, server-to-server; no lo llama ningún usuario final, por
eso no valida JWT. En producción se protegería a nivel de red o con autenticación
servicio-a-servicio (mTLS, client credentials). La justificación está comentada en
[`Program.cs`](src/PaymentService.WebApi/Program.cs).

## Cómo se integra con MiEcommerce

MiEcommerce llama a este servicio al confirmar una orden (`POST /api/orders/{id}/confirm`):

| Pieza | Archivo |
|---|---|
| Puerto | `MiEcommerce.Application/Common/Interfaces/IPaymentServiceClient.cs` |
| Implementación (typed HttpClient) | `MiEcommerce.Infrastructure/ExternalServices/Payment/PaymentServiceClient.cs` |
| Registro y resiliencia | `MiEcommerce.Infrastructure/DependencyInjection.cs` (`AddHttpClient` + `AddStandardResilienceHandler`) |
| Caso de uso | `MiEcommerce.Application/Features/Orders/ConfirmOrder/ConfirmOrderCommandHandler.cs` |

El flujo completo (aprobado, rechazado y PaymentService apagado) está documentado en el
[README de MiEcommerce](../Ecomerce/README.md).
