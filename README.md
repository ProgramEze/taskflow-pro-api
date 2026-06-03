# TaskFlow Pro API

TaskFlow Pro es una API REST desarrollada con ASP.NET Core para la gestión colaborativa de proyectos y tareas en equipos pequeños.

El proyecto permite registrar usuarios, iniciar sesión con JWT, crear espacios de trabajo, administrar proyectos, gestionar miembros, crear tareas, asignarlas a usuarios, modificar su estado y prioridad, realizar bajas lógicas, agregar comentarios, y consultar tareas mediante filtros, búsqueda y paginación.

Este proyecto forma parte de mi portfolio como desarrollador backend junior, aplicando buenas prácticas de arquitectura, autenticación, autorización, persistencia de datos, manejo global de errores, documentación de API, consultas paginadas, testing unitario e integration tests con base de datos real.

---

## Demo en producción

La API está desplegada en Railway y disponible públicamente:

```text
https://taskflow-pro-api-production.up.railway.app/swagger
```

---

## Tecnologías utilizadas

- C#
- ASP.NET Core 8
- Entity Framework Core
- PostgreSQL
- Docker
- Docker Compose
- Railway (deploy en la nube)
- JWT Authentication
- BCrypt
- Swagger / OpenAPI
- xUnit
- Moq
- FluentAssertions
- Microsoft.AspNetCore.Mvc.Testing
- Clean Architecture moderada

---

## Arquitectura del proyecto

El proyecto está organizado en capas siguiendo una Clean Architecture moderada:

```text
TaskFlowPro
├── TaskFlowPro.Api
├── TaskFlowPro.Application
├── TaskFlowPro.Domain
├── TaskFlowPro.Infrastructure
├── TaskFlowPro.Tests
└── TaskFlowPro.IntegrationTests
```

### TaskFlowPro.Api

Contiene la capa de presentación de la API:

- Controllers
- Configuración de Swagger
- Configuración de JWT
- Middleware global de errores
- Extensiones para obtener el usuario autenticado
- Configuración de dependencias

### TaskFlowPro.Application

Contiene la lógica de aplicación:

- DTOs
- Interfaces
- Services
- Excepciones personalizadas
- Modelos de respuesta paginada
- Servicio centralizado de autorización por workspace

### TaskFlowPro.Domain

Contiene el núcleo del dominio:

- Entidades
- Enums

### TaskFlowPro.Infrastructure

Contiene detalles técnicos:

- Entity Framework Core
- AppDbContext
- Repositories
- Generación de JWT
- Acceso a PostgreSQL

### TaskFlowPro.Tests

Contiene pruebas unitarias del proyecto:

- Tests con xUnit
- Mocks con Moq
- Aserciones con FluentAssertions
- Pruebas sobre lógica de aplicación

### TaskFlowPro.IntegrationTests

Contiene pruebas de integración:

- Tests con xUnit
- `CustomWebApplicationFactory` con base de datos de testing separada
- Migraciones aplicadas automáticamente al iniciar los tests
- Validación de arranque de la API y disponibilidad de Swagger
- Tests reales contra endpoints de Auth, Workspaces, Projects y Tasks con base de datos PostgreSQL

---

## Funcionalidades implementadas

### Auth

- Registro de usuarios.
- Login con validación de contraseña usando BCrypt.
- Generación de token JWT.
- Endpoint protegido para obtener el usuario autenticado.

Endpoints principales:

```http
POST /api/Auth/register
POST /api/Auth/login
GET /api/Auth/me
```

---

### Workspaces

- Crear workspace.
- Listar workspaces del usuario autenticado.
- Ver detalle de un workspace.
- Asignación automática del usuario creador como Owner.

Endpoints principales:

```http
POST /api/Workspaces
GET /api/Workspaces
GET /api/Workspaces/{workspaceId}
```

---

### Workspace Members

- Agregar miembros a un workspace por email.
- Listar miembros de un workspace.
- Cambiar rol de un miembro.
- Quitar miembros mediante baja lógica.
- Validación de permisos según rol.
- Solo Owner/Admin pueden agregar o quitar miembros.
- Solo Owner puede cambiar roles.
- No se puede quitar ni modificar el rol del Owner.
- Admin no puede quitar a otro Admin.

Endpoints principales:

```http
POST /api/workspaces/{workspaceId}/members
GET /api/workspaces/{workspaceId}/members
PUT /api/workspaces/{workspaceId}/members/{memberId}/role
DELETE /api/workspaces/{workspaceId}/members/{memberId}
```

---

### Projects

- Crear proyectos dentro de un workspace.
- Listar proyectos por workspace.
- Ver detalle de proyecto.
- Validación de pertenencia al workspace.
- Autorización centralizada por rol.
- Solo Owner/Admin pueden crear proyectos.
- Los miembros activos pueden consultar proyectos del workspace.

Endpoints principales:

```http
POST /api/workspaces/{workspaceId}/projects
GET /api/workspaces/{workspaceId}/projects
GET /api/projects/{projectId}
```

---

### Tasks

- Crear tareas dentro de un proyecto.
- Listar tareas por proyecto.
- Ver detalle de tarea.
- Cambiar estado de tarea.
- Cambiar prioridad de tarea.
- Editar tarea.
- Baja lógica de tarea.
- Asignar tareas a miembros activos del workspace.
- Filtrar tareas por estado.
- Filtrar tareas por prioridad.
- Filtrar tareas por usuario asignado.
- Buscar tareas por texto en título o descripción.
- Paginar resultados.
- Autorización centralizada por workspace.
- Solo Owner/Admin pueden asignar tareas.

Endpoints principales:

```http
POST /api/projects/{projectId}/tasks
GET /api/projects/{projectId}/tasks
GET /api/tasks/{taskId}
PATCH /api/tasks/{taskId}/status
PATCH /api/tasks/{taskId}/priority
PATCH /api/tasks/{taskId}/assign
PUT /api/tasks/{taskId}
DELETE /api/tasks/{taskId}
```

Ejemplo de listado con filtros y paginación:

```http
GET /api/projects/{projectId}/tasks?status=1&priority=3&pageNumber=1&pageSize=10
```

Parámetros disponibles:

```text
status          Filtra por estado de tarea
priority        Filtra por prioridad
assignedUserId  Filtra por usuario asignado
search          Busca texto en título o descripción
pageNumber      Número de página
pageSize        Cantidad de elementos por página
```

Ejemplo de respuesta paginada:

```json
{
    "items": [
        {
            "id": "609b1dec-95c8-4e19-b15f-f127b8f3e474",
            "projectId": "aed14075-fdf6-4417-8f06-baeed9196626",
            "title": "Diseñar endpoints de autenticación",
            "description": "Crear endpoints de registro, login y obtención del usuario autenticado",
            "status": 2,
            "priority": 3,
            "createdByUserId": "50185452-dfe4-4120-9fce-5b172bf78169",
            "assignedUserId": "50185452-dfe4-4120-9fce-5b172bf78169",
            "createdAt": "2026-05-31T23:43:50.562629Z",
            "dueDate": "2026-06-15T00:00:00Z",
            "isActive": true
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalItems": 1,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
}
```

---

### Comments

- Crear comentarios en tareas.
- Listar comentarios por tarea.
- Editar comentarios propios.
- Baja lógica de comentarios.
- Cualquier miembro activo del workspace puede ver y crear comentarios.
- Solo el autor del comentario puede editarlo.
- Solo el autor del comentario puede eliminarlo.
- Autorización centralizada por workspace.

Endpoints principales:

```http
POST /api/tasks/{taskId}/comments
GET /api/tasks/{taskId}/comments
PUT /api/comments/{commentId}
DELETE /api/comments/{commentId}
```

---

## Autorización por workspace

El proyecto incluye un servicio centralizado de autorización:

```text
WorkspaceAuthorizationService
```

Este servicio permite validar permisos de forma reutilizable en los servicios de aplicación.

Métodos principales:

```text
GetCurrentMemberOrThrow
EnsureMember
EnsureOwner
EnsureOwnerOrAdmin
```

Actualmente se utiliza en:

- `ProjectService`
- `TaskService`
- `WorkspaceMemberService`
- `CommentService`

Esto evita repetir lógica de permisos en cada servicio y facilita mantener reglas de negocio más claras.

---

## Modelo principal del sistema

El flujo principal del sistema es:

```text
User
  ↓
Workspace
  ↓
WorkspaceMember
  ↓
Project
  ↓
TaskItem
  ↓
Comment
```

Un usuario puede crear workspaces.
Un workspace contiene miembros.
Un workspace contiene proyectos.
Un proyecto contiene tareas.
Una tarea puede asignarse a un miembro activo del workspace.
Una tarea puede tener comentarios.

---

## Roles y permisos

### WorkspaceRole

```text
1 = Owner
2 = Admin
3 = Member
```

Reglas principales implementadas:

- El usuario que crea un workspace queda registrado automáticamente como Owner.
- Owner/Admin pueden agregar miembros.
- Owner/Admin pueden quitar miembros.
- Solo Owner puede cambiar roles.
- No se puede quitar al Owner del workspace.
- No se puede modificar el rol del Owner desde el endpoint de cambio de rol.
- Admin no puede quitar a otro Admin.
- Solo Owner/Admin pueden crear proyectos.
- Los miembros activos pueden ver proyectos y tareas del workspace.
- Los miembros activos pueden crear tareas.
- Una tarea solo puede asignarse a un usuario que sea miembro activo del workspace.
- Solo Owner/Admin pueden asignar tareas.
- Los miembros activos pueden crear y listar comentarios.
- Solo el autor puede editar o eliminar sus propios comentarios.

---

## Enums principales

### ProjectStatus

```text
1 = Active
2 = Paused
3 = Completed
4 = Archived
```

### TaskItemStatus

```text
1 = Pending
2 = InProgress
3 = InReview
4 = Completed
5 = Cancelled
```

### TaskPriority

```text
1 = Low
2 = Medium
3 = High
4 = Urgent
```

---

## Manejo global de errores

El proyecto incluye un middleware global para capturar excepciones y devolver respuestas consistentes.

Ejemplo de respuesta de error:

```json
{
    "statusCode": 400,
    "message": "El título de la tarea es obligatorio.",
    "detail": null,
    "path": "/api/projects/{projectId}/tasks",
    "timestamp": "2026-05-31T08:38:35Z"
}
```

Excepciones personalizadas implementadas:

- `BadRequestException`
- `UnauthorizedException`
- `ForbiddenException`
- `NotFoundException`
- `ConflictException`

---

## Base de datos

### Desarrollo local

El proyecto utiliza PostgreSQL levantado con Docker Compose.

El archivo `docker-compose.yml` levanta dos servicios:

- `taskflowpro-postgres`: base de datos principal, disponible en `localhost:5433`.
- `taskflowpro-postgres-test`: base de datos exclusiva para integration tests, disponible en `localhost:5434`.

Para iniciar ambas bases de datos:

```powershell
docker compose up -d
```

Para detener ambas bases de datos:

```powershell
docker compose down
```

Connection string utilizada en desarrollo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=taskflowpro_db;Username=postgres;Password=postgres"
}
```

### Producción (Railway)

En producción la base de datos es provisionada por Railway. La connection string se inyecta mediante la variable de entorno `ConnectionStrings__DefaultConnection` y las migraciones se aplican automáticamente al iniciar la aplicación.

### Base de datos de testing

- Base de datos: `taskflowpro_test_db`
- Puerto local: `5434`

Utilizada exclusivamente por `CustomWebApplicationFactory`. Las migraciones se aplican automáticamente al correr `dotnet test`.

---

## Deploy

La API está desplegada en Railway usando Docker.

### Infraestructura en producción

- **API**: contenedor Docker con ASP.NET Core 8, construido a partir del `Dockerfile` en la raíz del repositorio.
- **Base de datos**: PostgreSQL provisionado por Railway, conectado a la API mediante Variable References internas.
- **Migraciones**: se aplican automáticamente al arrancar la aplicación (`db.Database.Migrate()`).
- **Puerto**: Railway inyecta la variable `PORT`; la app la consume mediante `ASPNETCORE_URLS=http://+:${PORT}`.
- **Deploy continuo**: cada push a `main` dispara un redeploy automático.

### Variables de entorno requeridas en producción

```text
ASPNETCORE_ENVIRONMENT              = Production
ConnectionStrings__DefaultConnection = Host=...;Port=...;Database=...;Username=...;Password=...
Jwt__Key                            = <clave secreta>
Jwt__Issuer                         = TaskFlowPro
Jwt__Audience                       = TaskFlowProUsers
```

---

## Configuración JWT

Ejemplo de configuración en `appsettings.json`:

```json
"Jwt": {
  "Key": "taskflowpro_super_secret_key_for_development_2026",
  "Issuer": "TaskFlowPro",
  "Audience": "TaskFlowProUsers",
  "ExpirationMinutes": 60
}
```

> Nota: en producción la clave JWT se maneja mediante variables de entorno, no en `appsettings.json`.

---

## Migraciones

Crear una migración:

```powershell
dotnet ef migrations add InitialCreate --project .\TaskFlowPro.Infrastructure\TaskFlowPro.Infrastructure.csproj --startup-project .\TaskFlowPro.Api\TaskFlowPro.Api.csproj --output-dir Data\Migrations
```

Aplicar migraciones a la base de datos principal:

```powershell
dotnet ef database update --project .\TaskFlowPro.Infrastructure\TaskFlowPro.Infrastructure.csproj --startup-project .\TaskFlowPro.Api\TaskFlowPro.Api.csproj
```

> La base de datos de testing y la de producción reciben las migraciones automáticamente.

---

## Ejecutar el proyecto localmente

Desde la raíz de la solución:

```powershell
dotnet run --project .\TaskFlowPro.Api\TaskFlowPro.Api.csproj
```

Swagger estará disponible en:

```text
http://localhost:5052/swagger
```

---

## Testing

El proyecto incluye pruebas unitarias e integration tests con base de datos real.

Tecnologías utilizadas para testing:

- xUnit
- Moq
- FluentAssertions
- Microsoft.AspNetCore.Mvc.Testing
- WebApplicationFactory

Ejecutar todos los tests:

```powershell
dotnet test
```

### Unit tests

Actualmente se incluyen pruebas unitarias para:

#### TaskService

- Crear una tarea correctamente.
- Lanzar `NotFoundException` cuando el proyecto no existe.
- Lanzar `ForbiddenException` cuando el usuario no pertenece al workspace.
- Lanzar `BadRequestException` cuando el título de la tarea está vacío.
- Devolver tareas paginadas correctamente.

#### WorkspaceMemberService

- Agregar un miembro cuando el usuario actual es Owner.
- Lanzar `ForbiddenException` cuando un Member intenta agregar miembros.
- Lanzar `ConflictException` cuando el usuario ya pertenece al workspace.
- Cambiar el rol de un miembro cuando el usuario actual es Owner.
- Lanzar `ForbiddenException` cuando un Admin intenta cambiar roles.
- Lanzar `BadRequestException` al intentar modificar el rol del Owner.
- Quitar un miembro cuando el usuario actual es Owner.
- Lanzar `BadRequestException` al intentar quitar al Owner.

#### CommentService

- Crear un comentario cuando el usuario es miembro del workspace.
- Lanzar `NotFoundException` cuando la tarea no existe.
- Lanzar `BadRequestException` cuando el contenido del comentario está vacío.
- Listar comentarios activos de una tarea.
- Editar un comentario cuando el usuario es el autor.
- Lanzar `ForbiddenException` cuando un usuario intenta editar un comentario ajeno.
- Eliminar un comentario cuando el usuario es el autor.
- Lanzar `ForbiddenException` cuando un usuario intenta eliminar un comentario ajeno.

### Integration tests

Los integration tests levantan la API en memoria usando `CustomWebApplicationFactory`, que reemplaza la base de datos principal por `taskflowpro_test_db` y aplica las migraciones automáticamente antes de ejecutar los tests.

Cada test crea su propio `HttpClient` mediante `_factory.CreateClient()` para garantizar aislamiento total entre pruebas.

#### SwaggerTests

- Verificar que Swagger responda correctamente en `/swagger/v1/swagger.json`.

#### AuthIntegrationTests

- Registrar un usuario con datos válidos devuelve `201 Created`.
- Registrar un usuario con email duplicado devuelve `409 Conflict`.
- Registrar un usuario con contraseña vacía devuelve `400 Bad Request`.
- Hacer login con credenciales válidas devuelve `200 OK` con token en el body.

#### WorkspaceIntegrationTests

- Crear un workspace con datos válidos devuelve `201 Created`.
- Crear un workspace sin token devuelve `401 Unauthorized`.
- Crear un workspace con nombre vacío devuelve `400 Bad Request`.
- Listar workspaces del usuario autenticado devuelve `200 OK`.
- Listar workspaces sin token devuelve `401 Unauthorized`.

#### ProjectIntegrationTests

- Crear un proyecto con datos válidos devuelve `201 Created`.
- Crear un proyecto con fecha de vencimiento devuelve `201 Created`.
- Crear un proyecto sin token devuelve `401 Unauthorized`.
- Crear un proyecto con nombre vacío devuelve `400 Bad Request`.
- Crear un proyecto en un workspace inexistente devuelve `404 Not Found`.
- Listar proyectos del workspace con token devuelve `200 OK`.
- Listar proyectos sin token devuelve `401 Unauthorized`.
- Listar proyectos en un workspace inexistente devuelve `404 Not Found`.
- Obtener un proyecto por ID válido devuelve `200 OK`.
- Obtener un proyecto sin token devuelve `401 Unauthorized`.
- Obtener un proyecto con ID inexistente devuelve `404 Not Found`.

#### TaskIntegrationTests

- Crear una tarea con datos válidos devuelve `201 Created`.
- Crear una tarea con fecha de vencimiento devuelve `201 Created`.
- Crear una tarea sin token devuelve `401 Unauthorized`.
- Crear una tarea con título vacío devuelve `400 Bad Request`.
- Crear una tarea en un proyecto inexistente devuelve `404 Not Found`.
- Listar tareas del proyecto con token devuelve `200 OK`.
- Listar tareas sin token devuelve `401 Unauthorized`.
- Listar tareas en un proyecto inexistente devuelve `404 Not Found`.
- Obtener una tarea por ID válido devuelve `200 OK`.
- Obtener una tarea sin token devuelve `401 Unauthorized`.
- Obtener una tarea con ID inexistente devuelve `404 Not Found`.
- Cambiar el estado de una tarea devuelve `200 OK`.
- Cambiar el estado sin token devuelve `401 Unauthorized`.
- Cambiar el estado de una tarea inexistente devuelve `404 Not Found`.
- Cambiar la prioridad de una tarea devuelve `200 OK`.
- Cambiar la prioridad sin token devuelve `401 Unauthorized`.
- Actualizar una tarea con datos válidos devuelve `200 OK`.
- Actualizar una tarea con título vacío devuelve `400 Bad Request`.
- Actualizar una tarea sin token devuelve `401 Unauthorized`.
- Eliminar una tarea existente devuelve `204 No Content`.
- Eliminar una tarea sin token devuelve `401 Unauthorized`.
- Eliminar una tarea con ID inexistente devuelve `404 Not Found`.
- Eliminar una tarea ya eliminada (soft delete) devuelve `404 Not Found`.

Resultado esperado al correr `dotnet test`:

```text
TaskFlowPro.Tests              -> 21 tests correctos
TaskFlowPro.IntegrationTests   -> 42 tests correctos
Total general                  -> 63 tests correctos
```

---

## Pruebas realizadas

Se probaron los flujos principales desde Swagger:

- Registro de usuario.
- Login con JWT.
- Acceso a endpoint protegido `/me`.
- Creación y listado de workspaces.
- Creación y listado de proyectos.
- Creación, edición, cambio de estado, cambio de prioridad y baja lógica de tareas.
- Creación, edición, listado y baja lógica de comentarios.
- Gestión de miembros del workspace.
- Cambio de roles de miembros.
- Baja lógica de miembros.
- Asignación de tareas a miembros activos del workspace.
- Validación de error al intentar asignar una tarea a un usuario externo al workspace.
- Listado paginado de tareas.
- Filtros de tareas por estado, prioridad y usuario asignado.
- Búsqueda de tareas por texto.
- Validación de errores de paginación.
- Autorización centralizada en Projects, Tasks, Members y Comments.
- Manejo de errores mediante middleware global.
- Tests unitarios de lógica de aplicación.
- Integration tests con base de datos real para Auth, Workspaces, Projects y Tasks.
- Deploy en Railway con PostgreSQL y migraciones automáticas.

---

## Estado actual del proyecto

```text
Autenticación
  ↓
Workspaces
  ↓
Members
  ↓
Projects
  ↓
Tasks
  ↓
Filtering & Pagination
  ↓
Assignment
  ↓
Comments
  ↓
Centralized Authorization
  ↓
Unit Testing
  ↓
Integration Testing con base de datos real
  (Auth · Workspaces · Projects · Tasks)
  ↓
Deploy en Railway
  (Docker · PostgreSQL · CI/CD automático)
```

---

## Próximas mejoras

- Autoasignación de tareas para miembros.
- Frontend web.
