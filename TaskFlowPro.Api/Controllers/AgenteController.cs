using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlowPro.Application.Interfaces;

namespace TaskFlowPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgenteController : ControllerBase
{
    private readonly IAgenteServicio _agenteServicio;

    public AgenteController(IAgenteServicio agenteServicio)
    {
        _agenteServicio = agenteServicio;
    }

    [HttpPost("consultar")]
    public async Task<IActionResult> Consultar([FromBody] string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
            return BadRequest("El mensaje no puede estar vacío.");

        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var respuesta = await _agenteServicio.EjecutarAsync(mensaje, usuarioId);
        return Ok(respuesta);
    }
}
