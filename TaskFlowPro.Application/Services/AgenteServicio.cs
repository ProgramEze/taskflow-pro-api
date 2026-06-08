using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TaskFlowPro.Application.Interfaces;

namespace TaskFlowPro.Application.Services;

public class AgenteServicio : IAgenteServicio
{
    private readonly Kernel _kernel;

    public AgenteServicio(IConfiguration configuracion)
    {
        var apiKey = configuracion["OpenAI:ApiKey"]!;
        var modelId = configuracion["OpenAI:ModelId"]!;

        _kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId, apiKey)
            .Build();
    }

    public async Task<string> EjecutarAsync(string mensaje)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        var historial = new ChatHistory();
        historial.AddSystemMessage("Eres un asistente inteligente para gestionar tareas en TaskFlow Pro. Responde siempre en español.");
        historial.AddUserMessage(mensaje);

        var configuracionEjecucion = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var resultado = await chatCompletionService.GetChatMessageContentAsync(
            historial,
            executionSettings: configuracionEjecucion,
            kernel: _kernel
        );

        return resultado.Content ?? "Sin respuesta";
    }
}
