# 🏨 Hotel API - Sistema de Reservas

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)

Una API RESTful desarrollada en **C# y ASP.NET Core** para la gestión integral de un sistema de reservas de hotel. Este proyecto implementa una arquitectura híbrida (**CQRS Lite**), patrones de diseño modernos y optimización de consultas a base de datos.

## 🚀 Características Principales

*   **Arquitectura Híbrida (CQRS Lite):** Uso de **Entity Framework Core (ORM)** para garantizar la integridad en las escrituras (POST, PUT, DELETE) y **Dapper (MicroORM)** para lecturas (GET) de alto rendimiento.
*   **Manejo de Transacciones:** Implementación del principio ACID en la creación de reservas. Si una habitación o servicio falla, se ejecuta un `Rollback` automático, evitando registros huérfanos.
*   **Seguridad con DTOs:** Uso de *Data Transfer Objects* con **Data Annotations** (`[Required]`, `[Range]`) para prevenir ataques de *Over-Posting* y validar los datos antes de que lleguen al controlador.
*   **Middleware Global de Excepciones:** Interceptor de errores configurado para devolver respuestas JSON estandarizadas (HTTP 500) sin exponer la traza del servidor al cliente.
*   **Optimización de Consultas:** Uso de `.AsNoTracking()` e `.Include()` (Eager Loading) para evitar el problema N+1 y mejorar la velocidad de respuesta.
*   **SQL Avanzado:** Generación de reportes detallados (*Vouchers*) mediante consultas complejas con `STUFF` y `FOR XML PATH` para agrupar resultados.

## 🛠️ Tecnologías y Herramientas Utilizadas

*   **Lenguaje:** C# (.NET 8+)
*   **Framework Web:** ASP.NET Core Web API
*   **ORM:** Entity Framework Core (Code-First, Fluent API, Migrations)
*   **MicroORM:** Dapper
*   **Base de Datos:** SQL Server
*   **Documentación:** Swagger / OpenAPI
*   **Colecciones avanzadas:** Uso de `HashSet` para evitar duplicados en memoria y `Dictionary` para búsquedas O(1).

## 🗄️ Modelo de Datos (Relaciones)

El sistema cuenta con las siguientes entidades y relaciones configuradas con **Fluent API**:
*   `Usuario` (1 a N) `Reserva`
*   `Hotel` (1 a N) `Habitacion`
*   `Reserva` (1 a N) `ReservaDetalle` (Relación en Cascada)
*   `Reserva` (N a M) `ServicioAdicional` (A través de tabla intermedia `ReservaServicio`)

## ⚙️ Cómo ejecutar este proyecto localmente

1. Clona este repositorio:
   ```bash
   git clone https://github.com/TU_USUARIO/TU_REPOSITORIO.git
