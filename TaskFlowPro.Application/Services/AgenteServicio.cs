using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using TaskFlowPro.Application.Interfaces;

namespace TaskFlowPro.Application.Services;

public class AgenteServicio : IAgenteServicio
{
    private readonly Kernel _kernel;

    public AgenteServicio(IConfiguration configuracion)
    {
        var apiKey = configuracion["Gemini:ApiKey"]!;
        var modelId = configuracion["Gemini:ModelId"]!;

        _kernel = Kernel.CreateBuilder()
            .AddGoogleAIGeminiChatCompletion(modelId, apiKey)
            .Build();
    }

    public async Task<string> EjecutarAsync(string mensaje)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        var historial = new ChatHistory();
        historial.AddSystemMessage("Eres un asistente inteligente para gestionar tareas en TaskFlow Pro. Responde siempre en español.");
        historial.AddUserMessage(mensaje);

        var configuracionEjecucion = new GeminiPromptExecutionSettings
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
