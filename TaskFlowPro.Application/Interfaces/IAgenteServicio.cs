namespace TaskFlowPro.Application.Interfaces;

public interface IAgenteServicio
{
    Task<string> EjecutarAsync(string mensaje);
}
