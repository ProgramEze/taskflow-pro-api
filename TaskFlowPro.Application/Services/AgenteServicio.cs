using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using TaskFlowPro.Application.Agentes;
using TaskFlowPro.Application.Interfaces;

namespace TaskFlowPro.Application.Services;

public class AgenteServicio : IAgenteServicio
{
    private readonly IConfiguration _configuracion;
    private readonly ITaskService _taskService;

    public AgenteServicio(IConfiguration configuracion, ITaskService taskService)
    {
        _configuracion = configuracion;
        _taskService = taskService;
    }

    public async Task<string> EjecutarAsync(string mensaje, Guid usuarioActualId)
    {
        var apiKey = _configuracion["Gemini:ApiKey"]!;
        var modelId = _configuracion["Gemini:ModelId"]!;

        var kernel = Kernel.CreateBuilder()
            .AddGoogleAIGeminiChatCompletion(modelId, apiKey)
            .Build();

        // Registrar el plugin con el contexto del usuario actual
        kernel.Plugins.AddFromObject(new TareasPlugin(_taskService, usuarioActualId), "Tareas");

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var historial = new ChatHistory();
        historial.AddSystemMessage("Eres un asistente inteligente para gestionar tareas en TaskFlow Pro. Respondé siempre en español. Cuando el usuario pida realizar una acción, usá las herramientas disponibles.");
        historial.AddUserMessage(mensaje);

        var configuracionEjecucion = new GeminiPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var resultado = await chatCompletionService.GetChatMessageContentAsync(
            historial,
            executionSettings: configuracionEjecucion,
            kernel: kernel
        );

        return resultado.Content ?? "Sin respuesta";
    }
}
