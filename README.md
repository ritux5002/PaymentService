# PaymentService

Microservicio independiente que simula una pasarela de pagos. **No se integra ninguna
pasarela real** (Stripe, MercadoPago, etc.): la aprobación o el rechazo del pago los
decide el propio servicio con una regla local, explícita y determinística.

Arquitectura: Clean Architecture en 4 capas (Domain, Application, Infrastructure, WebApi),
con CQRS vía MediatR y FluentValidation — el mismo esquema que usa el proyecto principal
`MiEcommerce`, del que este servicio es totalmente independiente (solución, puertos y
ciclo de vida propios).

## Regla de aprobación (explícita)

Definida en [`PaymentService.Domain.Entities.Payment`](src/PaymentService.Domain/Entities/Payment.cs):

> Se **aprueba** el pago si el monto (`amount`) es **estrictamente menor a $100.000**.
> Se **rechaza** si el monto es **igual o mayor a $100.000**.

```csharp
public const decimal ApprovalLimit = 100_000m;
var status = amount < ApprovalLimit ? PaymentStatus.Approved : PaymentStatus.Rejected;
```

No hay validación de número de tarjeta ni ningún otro dato: el simulador solo mira el monto.
Cada intento (aprobado o rechazado) genera un `transactionId` único con formato `TX-XXXXXXXX`
y queda registrado en un repositorio en memoria (no hay base de datos: al reiniciar el
proceso se pierde el historial, algo aceptable para un simulador de este alcance).

## Endpoint

```
POST /api/payments/process
Content-Type: application/json

{ "orderId": "a1b2c3d4-...", "amount": 45000 }
```

Respuesta (200 OK) — aprobado:
```json
{ "status": "Approved", "transactionId": "TX-9F3C1A2B" }
```

Respuesta (200 OK) — rechazado (`amount = 150000`):
```json
{ "status": "Rejected", "transactionId": "TX-4D8E7F01" }
```

Errores de validación (`orderId` vacío o `amount <= 0`) devuelven `400 Bad Request`.

## Puerto y ejecución

| Perfil | URL |
|---|---|
| http | `http://localhost:5080` |
| https | `https://localhost:7080` |

```bash
dotnet run --project src/PaymentService.WebApi
```

Documentación interactiva en `http://localhost:5080/swagger`.

## Cómo se integra con MiEcommerce

El proyecto principal (`c:/Back/Ecomerce`) llama a este servicio por HTTP al **confirmar
una orden** (`POST /api/orders/{id}/confirm`), a través de:

- Puerto (contrato): `MiEcommerce.Application.Contracts.Infrastructure.IPaymentServiceClient`
- Implementación (HttpClient tipado): `MiEcommerce.Infrastructure.Services.PaymentServiceClient`
- Registro (`AddHttpClient`): `MiEcommerce.Infrastructure.InfrastructureServiceExtensions`
- Uso en el caso de uso: `MiEcommerce.Application.Features.Orders.Commands.ConfirmOrder.ConfirmOrderCommandHandler`

La URL base se configura en `MiEcommerce.WebApi/appsettings.json`:
```json
"PaymentService": { "BaseUrl": "http://localhost:5080/" }
```

### Flujo end-to-end

1. El cliente confirma una orden: `POST /api/orders/{orderId}/confirm` (e-commerce, requiere rol Admin).
2. El e-commerce reserva stock de cada ítem y pasa la orden a `Confirmed`.
3. El e-commerce llama a `POST http://localhost:5080/api/payments/process` con `{ orderId, amount: order.GetTotal() }`.
4. PaymentService responde `Approved` o `Rejected` según la regla de arriba.
5. El e-commerce actúa según la respuesta:
   - **Approved** → la orden pasa a `Paid` y guarda el `transactionId`. El stock reservado queda descontado (la venta se concreta). Desde `Paid` ya se puede `ship`/`deliver`.
   - **Rejected** → la orden pasa a `PaymentRejected`, guarda el `transactionId` del intento fallido, y el e-commerce **libera automáticamente** el stock que había reservado en el paso 2.
6. Si PaymentService no responde (apagado, timeout, error HTTP), el e-commerce devuelve `503 Service Unavailable` y **no persiste ningún cambio** (ni el estado `Confirmed` ni la reserva de stock) — la orden queda intacta en `Draft`, lista para reintentar el `confirm` más tarde.

## Cómo correr ambos servicios a la vez

```bash
# Terminal 1 — PaymentService
cd c:/Back/PaymentService
dotnet run --project src/PaymentService.WebApi

# Terminal 2 — MiEcommerce (requiere LocalDB con las migraciones aplicadas)
cd c:/Back/Ecomerce
dotnet run --project src/MiEcommerce.WebApi
```

Con ambos arriba, probar desde `http://localhost:8080/swagger`:

1. `POST /api/auth/login` con `admin@ecommerce.com` / `admin123` → tomar el token y pulsar **Authorize**.
2. `POST /api/customers/register` → crear un cliente.
3. `POST /api/products` → crear un producto con `price < 100000` (para ver el camino *Approved*) y otro con `price >= 100000` (para ver *Rejected*).
4. `POST /api/orders` → crear una orden con ese `customerId`.
5. `POST /api/orders/{orderId}/items` → agregar el producto.
6. `POST /api/orders/{orderId}/confirm` → observar la respuesta (`status: "Paid"` o `"PaymentRejected"` + `transactionId`).

El log de la consola de PaymentService muestra cada `ProcessPaymentCommand` recibido,
confirmando la comunicación entre ambos procesos independientes.
