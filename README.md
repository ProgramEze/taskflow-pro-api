# TaskFlow Pro API

TaskFlow Pro es una API REST desarrollada con ASP.NET Core para la gestión colaborativa de proyectos y tareas en equipos pequeños.

El proyecto permite registrar usuarios, iniciar sesión con JWT, crear espacios de trabajo, administrar proyectos, gestionar miembros, crear tareas, asignarlas a usuarios, modificar su estado y prioridad, realizar bajas lógicas y agregar comentarios.

Este proyecto forma parte de mi portfolio como desarrollador backend junior, aplicando buenas prácticas de arquitectura, autenticación, autorización, persistencia de datos, manejo global de errores y documentación de API.

---

## Tecnologías utilizadas

- C#
- ASP.NET Core 8
- Entity Framework Core
- PostgreSQL
- Docker
- Docker Compose
- JWT Authentication
- BCrypt
- Swagger / OpenAPI
- Clean Architecture moderada

---

## Arquitectura del proyecto

El proyecto está organizado en capas siguiendo una Clean Architecture moderada:

```text
TaskFlowPro
├── TaskFlowPro.Api
├── TaskFlowPro.Application
├── TaskFlowPro.Domain
└── TaskFlowPro.Infrastructure
```

### TaskFlowPro.Api

Contiene la capa de presentación de la API:

- Controllers
- Configuración de Swagger
- Configuración de JWT
- Middleware global de errores
- Extensiones para obtener el usuario autenticado

### TaskFlowPro.Application

Contiene la lógica de aplicación:

- DTOs
- Interfaces
- Services
- Excepciones personalizadas

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

---

### Comments

- Crear comentarios en tareas.
- Listar comentarios por tarea.
- Editar comentarios propios.
- Baja lógica de comentarios.

Endpoints principales:

```http
POST /api/tasks/{taskId}/comments
GET /api/tasks/{taskId}/comments
PUT /api/comments/{commentId}
DELETE /api/comments/{commentId}
```

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
- Una tarea solo puede asignarse a un usuario que sea miembro activo del workspace.

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

El proyecto utiliza PostgreSQL levantado con Docker Compose.

Para iniciar la base de datos:

```powershell
docker compose up -d
```

Para detener la base de datos:

```powershell
docker compose down
```

El servicio PostgreSQL queda disponible en:

```text
localhost:5433
```

El archivo `docker-compose.yml` crea:

- Base de datos: `taskflowpro_db`
- Usuario: `postgres`
- Password: `postgres`
- Puerto local: `5433`

Connection string utilizada en desarrollo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=taskflowpro_db;Username=postgres;Password=postgres"
}
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

> Nota: en un entorno productivo, la clave JWT no debería quedar escrita directamente en el archivo `appsettings.json`. Debería manejarse mediante variables de entorno o secretos del entorno de despliegue.

---

## Migraciones

Crear una migración:

```powershell
dotnet ef migrations add InitialCreate --project .\TaskFlowPro.Infrastructure\TaskFlowPro.Infrastructure.csproj --startup-project .\TaskFlowPro.Api\TaskFlowPro.Api.csproj --output-dir Data\Migrations
```

Aplicar migraciones a la base de datos:

```powershell
dotnet ef database update --project .\TaskFlowPro.Infrastructure\TaskFlowPro.Infrastructure.csproj --startup-project .\TaskFlowPro.Api\TaskFlowPro.Api.csproj
```

---

## Ejecutar el proyecto

Desde la raíz de la solución:

```powershell
dotnet run --project .\TaskFlowPro.Api\TaskFlowPro.Api.csproj
```

Swagger estará disponible en:

```text
http://localhost:5052/swagger
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
- Manejo de errores mediante middleware global.

---

## Estado actual del proyecto

Proyecto en desarrollo.

Actualmente implementa el flujo principal:

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
Assignment
  ↓
Comments
```

---

## Próximas mejoras

- Permisos más específicos por rol.
- Autoasignación de tareas para miembros.
- Filtros y paginación.
- Tests unitarios.
- Tests de integración.
- Deploy en la nube.
- Frontend web.
