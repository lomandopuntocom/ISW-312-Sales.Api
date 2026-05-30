# RECUPERATORIO — Integración Inventario ↔ Ventas

Este documento contiene la documentación requerida para la entrega de la segunda instancia de evaluación (Recuperatorio) de la asignatura **ISW-312**.

---

## 3.1 — Estructura del proyecto

El módulo de **Ventas (Sales.Api)** está diseñado siguiendo una arquitectura limpia y modularizada:

```
Sales.Api/
├── Controllers/
│   ├── CatalogController.cs
│   ├── DashboardController.cs
│   ├── KdsController.cs
│   ├── KdsItemsController.cs
│   ├── SalesControllerBase.cs
│   ├── SettingsController.cs
│   ├── TicketsController.cs
│   └── WaitersController.cs
├── Application/
│   ├── Abstractions/
│   │   └── IInventoryCatalogClient.cs
│   └── Dtos/
│       ├── CatalogProductDto.cs
│       ├── TicketDto.cs
│       └── ...
├── Domain/
│   └── Entities/
│       ├── Command.cs
│       ├── CommandItem.cs
│       ├── Payment.cs
│       ├── SalesCompany.cs
│       ├── SalesLocation.cs
│       ├── Ticket.cs
│       ├── TicketItem.cs
│       └── Vendor.cs
├── Infrastructure/
│   ├── Inventory/
│   │   └── InventoryCatalogClient.cs
│   └── Persistence/
│       └── SalesDbContext.cs
├── Program.cs
└── appsettings.json
```

### Flujo de una Venta en el Sistema
1. El cliente inicia una venta creando una cuenta/ticket con una llamada a `POST /api/sales/companies/{companyCen}/tickets` (manejado por `TicketsController.CreateTicket`), el cual se registra en base de datos local como `OPEN`.
2. Se agregan ítems al ticket a través del endpoint `POST .../tickets/{ticketCen}/items`. En este punto, Ventas consulta dinámicamente al servicio de **Inventario** (`LookupProductsAsync` y `ValidateStockAsync`) para verificar que el producto exista y que cuente con stock suficiente antes de agregarlo localmente.
3. Al pagar la cuenta en `POST .../tickets/{ticketCen}/payment`, Ventas realiza una llamada HTTP al Inventario (`ConsumeStockAsync`) por cada ítem. Si el Inventario descuenta el stock de manera exitosa, Ventas registra el pago localmente y cambia el estado del ticket a `PAID`.

---

## 3.2 — Integración con Inventario

### 3.2.1 — Llamada HTTP al Inventario
Las llamadas HTTP externas hacia el Inventario se realizan a través de la clase `InventoryCatalogClient.cs` (ubicada en `Sales.Api.Infrastructure.Inventory`).

**Snippet relevante de la llamada para consumir stock:**
```csharp
public async Task<bool> ConsumeStockAsync(string companyCen, string productCen, decimal quantity, CancellationToken cancellationToken)
{
    var response = await httpClient.PostAsJsonAsync(
        $"/api/inventory/companies/{companyCen}/stock/consume",
        new { productCen, quantity, reference = "SALES", notes = "Ticket payment" },
        cancellationToken);

    return response.IsSuccessStatusCode;
}
```

---

### 3.2.2 — Configuración de la URL de Inventario
La URL base del servicio de Inventario se define en el archivo de variables de entorno `.env` en la raíz del proyecto y se inyecta en el contenedor de servicios de ASP.NET Core.

**Fragmento de `.env.example`:**
```ini
# Services Configuration
INVENTORY_API_URL=http://localhost:5143
```

**Fragmento de código en `Program.cs` que lee la variable y configura el cliente HTTP:**
```csharp
var inventoryBaseUrl = Environment.GetEnvironmentVariable("INVENTORY_API_URL")
    ?? "http://localhost:5143";

builder.Services.AddHttpClient<IInventoryCatalogClient, InventoryCatalogClient>(client =>
{
    client.BaseAddress = new Uri(inventoryBaseUrl);
});
```

---

### 3.2.3 — Manejo de errores del Inventario (404 y 500)
* **Si el Inventario devuelve 404 (Producto no encontrado):** El método `LookupProductsAsync` devuelve un listado vacío. En `TicketsController.AddItem`, se evalúa si el producto es nulo y se devuelve un `404 Not Found` al cliente.
* **Si el Inventario devuelve 500 o falla la conexión:** El cliente HTTP devuelve códigos no exitosos y `ConsumeStockAsync` o `ValidateStockAsync` retornan `false`. El controlador de Ventas responde con un código de error de conflicto `409 Conflict`.

**Snippet de control en `AddItem` (dentro de `TicketsController.cs`):**
```csharp
var products = await inventoryClient.LookupProductsAsync(companyCen, [request.ProductCen], cancellationToken);
var product = products.FirstOrDefault();
if (product is null) return NotFound("Product not found in inventory.");
```

**Snippet de consumo en `Pay` (dentro de `TicketsController.cs`):**
```csharp
foreach (var item in ticket.Items)
{
    var consumed = await inventoryClient.ConsumeStockAsync(companyCen, item.ProductCen.ToString(), item.Quantity, cancellationToken);
    if (!consumed) return Conflict($"Insufficient stock or inventory error for product {item.ProductCen}.");
}
```

---

## 3.3 — Preguntas teóricas

### Pregunta A — Cambio en el contrato
Si el compañero renombra el campo `"cantidad"` por `"qty"`, se generará un error de deserialización en nuestro DTO que mapeará el stock a `0` o `null`, provocando que Ventas asuma erróneamente que no hay unidades disponibles y rechace compras válidas. Para mitigar este riesgo, se debe acordar un esquema de versionamiento de APIs (manteniendo la ruta anterior activa al lanzar la nueva) e implementar pruebas automatizadas de contrato que validen la estructura JSON recibida antes de desplegar cambios a producción.

### Pregunta B — Red caída a mitad de una transacción
Una caída de red tras procesar el descuento en el Inventario pero antes de recibir confirmación en Ventas causa un problema de inconsistencia de stock duplicado (el Inventario restó stock pero Ventas no cobró la cuenta). Este problema se detecta mediante el control de excepciones de conexión HTTP (`SocketException`/timeout) y se maneja mediante endpoints idempotentes en el Inventario: al enviar el identificador único del ticket (`Cen`) en cada petición de consumo, el Inventario puede validar si ya procesó dicha transacción previamente y responder con éxito en lugar de duplicar el descuento de stock.

### Pregunta C — Inventario caído
El sistema de Ventas actual prioriza la consistencia estricta rechazando el registro de pagos (retornando un error **HTTP 409 Conflict**) si el Inventario no responde. La ventaja de este enfoque es que garantiza la exactitud del stock previniendo la venta de productos sin existencias físicas reales, pero la desventaja es que paraliza por completo la operación comercial del restaurante ante cualquier caída de red o de los servicios del compañero.

### Pregunta D — URL hardcodeada
Tener escrita la URL del Inventario directamente en el código de Ventas es un problema de portabilidad porque impide cambiar la dirección de red según el entorno de ejecución (desarrollo, pruebas, producción) sin modificar el código fuente y volver a compilar el proyecto. Esto se resolvió abstrayendo la dirección a la variable de entorno `INVENTORY_API_URL` en el archivo `.env`, la cual se lee dinámicamente mediante `Environment.GetEnvironmentVariable` al configurar el `HttpClient` de la aplicación.
