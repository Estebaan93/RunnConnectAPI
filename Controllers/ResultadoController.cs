// Controllers/ResultadoController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunnConnectAPI.Models.Dto.Resultado;
using RunnConnectAPI.Repositories;
using System.Security.Claims;

namespace RunnConnectAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ResultadoController : ControllerBase
  {
    private readonly ResultadoRepositorio _resultadoRepo;

    public ResultadoController(ResultadoRepositorio resultadoRepo)
    {
      _resultadoRepo = resultadoRepo;
    }

    // ═══════════════════ ENDPOINTS PUBLICOS ═══════════════════

    /// Obtiene un resultado por ID
    /// Endpoint público - cualquiera puede ver los resultados

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
      try
      {
        var resultado = await _resultadoRepo.ObtenerPorIdAsync(id);

        if (resultado == null)
          return NotFound(new { message = "Resultado no encontrado" });

        return Ok(resultado);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error al obtener el resultado", error = ex.Message });
      }
    }


    /// Obtiene todos los resultados de un evento


    /// Endpoint público - muestra tabla de posiciones
    /// Ordenado por posición general
    [HttpGet("Evento/{idEvento}")]
    public async Task<IActionResult> ObtenerResultadosEvento(int idEvento)
    {
      try
      {
        var resultados = await _resultadoRepo.ObtenerResultadosEventoAsync(idEvento);

        if (resultados == null)
          return NotFound(new { message = "Evento no encontrado" });

        return Ok(resultados);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error al obtener resultados del evento", error = ex.Message });
      }
    }

    /// Obtiene resultados de una categoría específica
    [HttpGet("Categoria/{idCategoria}")]
    public async Task<IActionResult> ObtenerResultadosCategoria(int idCategoria)
    {
      try
      {
        var resultados = await _resultadoRepo.ObtenerResultadosPorCategoriaAsync(idCategoria);
        return Ok(resultados);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error al obtener resultados de la categoría", error = ex.Message });
      }
    }


    // ═══════════════════ ENDPOINTS RUNNER ═══════════════════

    /// Obtiene los resultados del runner autenticado
    /// Requiere: Token JWT de Runner
    /// Incluye estadísticas acumuladas
    [HttpGet("MisResultados")]
    [Authorize]
    public async Task<IActionResult> MisResultados()
    {
      try
      {
        var (userId, error) = ValidarRunner();
        if (error != null) return error;

        var resultados = await _resultadoRepo.ObtenerMisResultadosAsync(userId);
        return Ok(resultados);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error al obtener tus resultados", error = ex.Message });
      }
    }

    /// Agrega datos de smartwatch/Google Fit a un resultado
    /// Requiere: Token JWT de Runner (dueño del resultado)
    /// Permite agregar métricas personales post-carrera
    [HttpPut("{id}/DatosSmartwatch")]
    [Authorize]
    public async Task<IActionResult> AgregarDatosSmartwatch(int id, [FromBody] DatosSmartwatchRequest request)
    {
      try
      {
        var (userId, error) = ValidarRunner();
        if (error != null) return error;

        var (exito, errorMsg) = await _resultadoRepo.AgregarDatosSmartwatchAsync(id, request, userId);

        if (!exito)
          return BadRequest(new { message = errorMsg });

        return Ok(new { message = "Datos de smartwatch agregados correctamente" });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error al agregar datos de smartwatch", error = ex.Message });
      }
    }


    // ═══════════════════ ENDPOINTS ORGANIZADOR ═══════════════════

    /// Carga un resultado individual
    /// Requiere: Token JWT de Organizador (dueño del evento)
    /// El evento debe estar en estado "finalizado"
    /// La inscripción debe estar "confirmada"
    [HttpPost("Cargar")]
    [Authorize]
    public async Task<IActionResult> CargarResultado([FromBody] CargarResultadoRequest request)
    {
      try
      {
        var (userId, error) = ValidarOrganizador();
        if (error != null) return error;

        var (resultado, errorMsg) = await _resultadoRepo.CargarResultadoAsync(request, userId);

        if (resultado == null)
          return BadRequest(new { message = errorMsg });

        return CreatedAtAction(
          nameof(ObtenerPorId),
          new { id = resultado.IdResultado },
          new
          {
            message = "Resultado cargado correctamente",
            idResultado = resultado.IdResultado
          });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error al cargar el resultado", error = ex.Message });
      }
    }

    /// Carga resultados en batch (múltiples runners)
    /// Requiere: Token JWT de Organizador (dueño del evento)
    /// Identifica runners por DNI
    /// Devuelve resumen de éxitos y errores
    [HttpPost("CargarBatch")]
    [Authorize]
    public async Task<IActionResult> CargarResultadosBatch([FromBody] CargarResultadosRequest request)
    {
      try
      {
        var (userId, error) = ValidarOrganizador();
        if (error != null) return error;

        var resultado = await _resultadoRepo.CargarResultadosBatchAsync(request, userId);

        if (resultado.Fallidos > 0 && resultado.Exitosos == 0)
          return BadRequest(new
          {
            message = "No se pudo cargar ningún resultado",
            detalles = resultado
          });

        return Ok(new
        {
          message = $"Carga completada: {resultado.Exitosos} exitosos, {resultado.Fallidos} fallidos",
          detalles = resultado
        });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error en la carga batch", error = ex.Message });
      }
    }

    /// Actualiza el tiempo oficial de un resultado
    [HttpPut("{id}/TiempoOficial")]
    [Authorize]
    public async Task<IActionResult> ActualizarTiempoOficial(int id, [FromBody] ActualizarTiempoOficialRequest request)
    {
      try
      {
        var (userId, error) = ValidarOrganizador();
        if (error != null) return error;

        var (exito, errorMsg) = await _resultadoRepo.ActualizarTiempoOficialAsync(id, request.TiempoOficial, userId);

        if (!exito)
          return BadRequest(new { message = errorMsg });

        return Ok(new { message = "Tiempo oficial actualizado correctamente" });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error al actualizar tiempo oficial", error = ex.Message });
      }
    }

    /// Actualiza las posiciones de un resultado
    [HttpPut("{id}/Posiciones")]
    [Authorize]
    public async Task<IActionResult> ActualizarPosiciones(int id, [FromBody] ActualizarPosicionesRequest request)
    {
      try
      {
        var (userId, error) = ValidarOrganizador();
        if (error != null) return error;

        var (exito, errorMsg) = await _resultadoRepo.ActualizarPosicionesAsync(id, request, userId);

        if (!exito)
          return BadRequest(new { message = errorMsg });

        return Ok(new { message = "Posiciones actualizadas correctamente" });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error al actualizar posiciones", error = ex.Message });
      }
    }

    /// Elimina un resultado
    /// Elimina completamente el resultado (incluyendo datos de smartwatch)
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> EliminarResultado(int id)
    {
      try
      {
        var (userId, error) = ValidarOrganizador();
        if (error != null) return error;

        var (exito, errorMsg) = await _resultadoRepo.EliminarResultadoAsync(id, userId);

        if (!exito)
          return BadRequest(new { message = errorMsg });

        return Ok(new { message = "Resultado eliminado correctamente" });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { message = "Error al eliminar resultado", error = ex.Message });
      }
    }


    // ═══════════════════ HELPERS PRIVADOS ═══════════════════

    private (int userId, IActionResult? error) ValidarRunner()
    {
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
      if (userIdClaim == null)
        return (0, Unauthorized(new { message = "No autorizado" }));

      var userId = int.Parse(userIdClaim.Value);

      var tipoUsuarioClaim = User.FindFirst("TipoUsuario");
      if (tipoUsuarioClaim == null || tipoUsuarioClaim.Value.ToLower() != "runner")
        return (0, BadRequest(new { message = "Solo los runners pueden realizar esta acción" }));

      return (userId, null);
    }

    private (int userId, IActionResult? error) ValidarOrganizador()
    {
      var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
      if (userIdClaim == null)
        return (0, Unauthorized(new { message = "No autorizado" }));

      var userId = int.Parse(userIdClaim.Value);

      var tipoUsuarioClaim = User.FindFirst("TipoUsuario");
      if (tipoUsuarioClaim == null || tipoUsuarioClaim.Value.ToLower() != "organizador")
        return (0, BadRequest(new { message = "Solo los organizadores pueden realizar esta acción" }));

      return (userId, null);
    }

  }

  /// DTO simple para actualizar tiempo oficial
  public class ActualizarTiempoOficialRequest
  {
    public string TiempoOficial { get; set; } = string.Empty;
  }
}