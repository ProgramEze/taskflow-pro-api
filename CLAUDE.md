# TaskFlow Pro API — Contexto de Ezequiel

## Quién soy
Developer junior orientado a backend. Estoy construyendo TaskFlow Pro API
como proyecto portfolio para conseguir mi primer trabajo en la industria.
También doy tutorías privadas (matemática, lengua e inglés) a una alumna
de 11-12 años con dificultades de atención.

Tengo experiencia previa desarrollando software freelance para clientes reales.
Proyecto de referencia: MisTresSoles — sistema de gestión para verdulería/almacén
(Java 21 + JavaFX + SQLite + MVC), con propuesta técnica y comercial incluida.
Ese proyecto demuestra capacidad de: diseñar BD desde cero, construir UI de escritorio,
pensar en UX para usuarios no técnicos, y presentar soluciones a clientes.

## Stack tecnológico
- Lenguaje: C# / ASP.NET Core
- Base de datos: PostgreSQL
- Contenedores: Docker / Docker Compose
- ORM: Entity Framework Core
- Tests: xUnit + integración con CustomWebApplicationFactory
- Deploy: Railway (producción activa)
- Control de versiones: GitHub (github.com/ProgramEze)

## Arquitectura
Clean Architecture con las siguientes capas:
- Domain → entidades y contratos
- Application → casos de uso
- Infrastructure → repositorios y acceso a datos
- API → controllers y configuración

## Convenciones de código
- Idioma del código: español (nombres de clases, métodos, variables, comentarios)
- Nomenclatura de variables y métodos: camelCase
- Nomenclatura de clases e interfaces: PascalCase (estándar C#)
- Nunca modificar archivos .env sin consultar primero
- Nunca instalar paquetes NuGet sin consultarme primero
- Pedir aprobación antes de eliminar cualquier archivo

## Flujo de trabajo para features nuevos ⭐
Ante cualquier feature nuevo o tarea ambigua, SIEMPRE hacer lo siguiente
antes de escribir código:

1. Proponer un plan detallado con pasos concretos
2. Esperar aprobación o ajustes de Ezequiel
3. Recién entonces implementar, paso a paso

Esto es importante: Ezequiel ejecuta muy bien cuando las pautas son claras,
pero le cuesta arrancar desde la ambigüedad. Claude actúa como el que
descompone el problema y propone la estructura.

## Orden de implementación de endpoints (SIEMPRE respetar)
1. Entidad en Domain
2. Repositorio en Infrastructure
3. Caso de uso en Application
4. Controller en API
5. Test de integración en Tests/

## Estado actual del proyecto
- 31 tests pasando (unit + integración)
- Endpoints implementados: Auth, Workspace, Project
- En producción: https://taskflow-pro-api-production.up.railway.app/swagger
- Próximos features: task self-assignment para workspace members, frontend web

## Archivos protegidos (deny list)
- .env y .env.local → nunca leer ni modificar
- secrets/ → nunca acceder
- Cualquier archivo con extensión *.pem

## Cómo trabajás mejor
- Ejecutás muy bien cuando las pautas son claras y los pasos están definidos
- Te cuesta arrancar cuando el problema es abierto o ambiguo (sin requisitos claros)
- Tenés experiencia trabajando con clientes no técnicos: sabés traducir necesidades
  de negocio a soluciones técnicas concretas
- Aprendés haciendo: preferís práctica con explicación sobre la marcha que teoría pura

## Cómo comunicarse conmigo
- Siempre responder en español
- Ser directo y concreto — no explicaciones largas si no las pido
- Ante errores, primero diagnosticar la causa raíz antes de proponer solución
- Si algo no está claro, preguntar antes de asumir
