# Sistema de Gestión de Ventas e Inventario

Sistema web desarrollado con ASP.NET Core 8 MVC, C# y SQL Server.

## Tecnologías
- ASP.NET Core 8 MVC
- Entity Framework Core 8 (Code First)
- ASP.NET Core Identity (autenticación y roles)
- SQL Server
- Bootstrap 5
- Chart.js

## Módulos
- Autenticación con roles (Administrador / Vendedor)
- Gestión de Categorías
- Gestión de Productos con imágenes
- Gestión de Clientes con validación de DNI único
- Ventas con carrito en sesión y transacciones SQL
- Reportes con gráficos (Chart.js)

## Cómo ejecutar localmente
1. Clonar el repositorio
2. Configurar la cadena de conexión en `appsettings.json`
3. Ejecutar migraciones: `Update-Database` en la consola de NuGet
4. Correr el proyecto con `Ctrl + F5`

## Credenciales por defecto
- **Email:** admin@gestionventas.com
- **Password:** Admin123
