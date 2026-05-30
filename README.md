# Módulo de Ventas (Sales.Api)

Este proyecto es la API de backend para el módulo de Ventas y Punto de Venta (POS), integrada con el módulo de Inventario. Está construida sobre **ASP.NET Core 9.0** y **Entity Framework Core**.

---

## 🛠️ Requisitos Previos

Asegúrate de tener instalado:
* [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
* [PostgreSQL](https://www.postgresql.org/download/)
* [pgAdmin 4](https://www.pgadmin.org/download/) (o herramienta de administración similar)
* Herramientas de EF Core (`dotnet tool install --global dotnet-ef`).
* **Módulo de Inventario levantado y corriendo** (por defecto en `http://localhost:5143`).

---

## 🚀 Guía de Instalación y Ejecución Paso a Paso

### Paso 1: Configurar la Base de Datos en pgAdmin
*(Si ya completaste el paso a paso del README de Inventario, puedes saltar al Paso 2).*
1. Abre **pgAdmin** y conéctate a tu servidor local de PostgreSQL.
2. Abre la consola de consultas (**Query Tool**) sobre el servidor o la base de datos por defecto (`postgres`).
3. Ejecuta el contenido del script localizado en el repositorio de **Inventario** (`database/create-database.sql` o `Inventory.Api/database/create-database.sql`) para crear la base de datos `ISW-312-PROJ1` y sus esquemas.

### Paso 2: Configurar las Variables de Entorno
1. En la raíz del repositorio, copia el archivo de plantilla `.env.example` y nómbralo como `.env`.
2. Asegúrate de configurar la URL del API de Inventario (`INVENTORY_API_URL`) y tus credenciales de PostgreSQL en `.env`:
   ```ini
   DATABASE_HOST=localhost
   DATABASE_PORT=5432
   DATABASE_NAME=ISW-312-PROJ1
   DATABASE_USER=tu_usuario_postgres
   DATABASE_PASSWORD=tu_contraseña_postgres
   
   INVENTORY_API_URL=http://localhost:5143
   ```

### Paso 3: Aplicar las Migraciones (Creación de Tablas)
Desde la terminal en la raíz del proyecto global (donde se encuentra el archivo `.sln`), ejecuta el comando para crear las tablas específicas del módulo de Ventas en la base de datos:

```bash
dotnet ef database update --project Sales.Api --context SalesDbContext
```

### Paso 4: Poblar la Base de Datos con Datos Semilla (Seed)
*(Si ya ejecutaste este script en el paso a paso de Inventario, puedes saltarlo).*
1. En pgAdmin, conéctate a la base de datos recién creada **`ISW-312-PROJ1`**.
2. Abre el **Query Tool** en esta base de datos.
3. Ejecuta por completo el script SQL de datos iniciales ubicado en el repositorio de **Inventario** (`database/seed.sql` o `Inventory.Api/database/seed.sql`).

### Paso 5: Ejecutar la API de Ventas
1. Desde tu terminal, navega a la carpeta del proyecto de Ventas:
   ```bash
   cd Sales.Api
   ```
2. Ejecuta la aplicación:
   ```bash
   dotnet run
   ```
3. El servicio levantará y podrás acceder a la documentación interactiva en:
   * **Swagger UI (Clásico):** [http://localhost:5038/swagger](http://localhost:5038/swagger)
   * **Scalar UI (Moderno):** [http://localhost:5038/scalar/v1](http://localhost:5038/scalar/v1)
   * **Especificación OpenAPI JSON:** [http://localhost:5038/openapi/v1.json](http://localhost:5038/openapi/v1.json)
