using System.ComponentModel;
using Microsoft.SemanticKernel;
using TaskFlowPro.Application.DTOs.Tasks;
using TaskFlowPro.Application.Interfaces;
using TaskFlowPro.Domain.Enums;

namespace TaskFlowPro.Application.Agentes;

public class TareasPlugin
{
    private readonly ITaskService _taskService;
    private readonly Guid _usuarioActualId;

    public TareasPlugin(ITaskService taskService, Guid usuarioActualId)
    {
        _taskService = taskService;
        _usuarioActualId = usuarioActualId;
    }

    [KernelFunction("ObtenerTareasAsignadas")]
    [Description("Obtiene todas las tareas asignadas al usuario actual")]
    public async Task<string> ObtenerTareasAsignadasAsync()
    {
        var tareas = await _taskService.ObtenerTareasAsignadasAsync(_usuarioActualId);

        if (!tareas.Any())
            return "No tenés tareas asignadas actualmente.";

        var resultado = tareas.Select(t =>
            $"- [{t.Status}] {t.Title} (Prioridad: {t.Priority}, ID: {t.Id})");

        return $"Tus tareas asignadas:\n{string.Join("\n", resultado)}";
    }

    [KernelFunction("CrearTarea")]
    [Description("Crea una nueva tarea en un proyecto. Requiere el ID del proyecto, título, y opcionalmente descripción y prioridad (Low, Medium, High)")]
    public async Task<string> CrearTareaAsync(
        [Description("ID del proyecto donde se creará la tarea")] Guid proyectoId,
        [Description("Título de la tarea")] string titulo,
        [Description("Descripción opcional de la tarea")] string? descripcion = null,
        [Description("Prioridad: Low, Medium o High")] string prioridad = "Medium")
    {
        var priority = Enum.TryParse<TaskPriority>(prioridad, true, out var p) ? p : TaskPriority.Medium;

        var request = new CreateTaskRequest
        {
            Title = titulo,
            Description = descripcion,
            Priority = priority
        };

        var tarea = await _taskService.CreateAsync(_usuarioActualId, proyectoId, request);
        return $"Tarea creada exitosamente: '{tarea.Title}' (ID: {tarea.Id})";
    }

    [KernelFunction("AsignarTarea")]
    [Description("Asigna una tarea existente a un usuario. Requiere el ID de la tarea y el ID del usuario")]
    public async Task<string> AsignarTareaAsync(
        [Description("ID de la tarea a asignar")] Guid tareaId,
        [Description("ID del usuario al que se asignará la tarea")] Guid usuarioId)
    {
        var request = new AssignTaskRequest { AssignedToId = usuarioId };
        var tarea = await _taskService.AssignAsync(_usuarioActualId, tareaId, request);
        return $"Tarea '{tarea.Title}' asignada correctamente al usuario {usuarioId}.";
    }
}
