// Controllers/EventoController.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunnConnectAPI.Models;
using RunnConnectAPI.Models.Dto.Evento;
using RunnConnectAPI.Repositories;
using System.Security.Claims;

namespace RunnConnectAPI.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class EventoController : ControllerBase
  {
    private readonly EventoRepositorio _eventoRepositorio;
    public EventoController(EventoRepositorio eventoRepositorio)
    {
      _eventoRepositorio= eventoRepositorio;
    }

    /*Endpoint publicos (sin autenticacion) para el usuario nuevo antes de loguearse, pueda ver los proximos eventos, y posterior
    decide si loguearse o no
    GET: api/Evento/Publicados - obtiene los eventos publicado y futuros*/
    [AllowAnonymous]
    [HttpGet("Publicados")]
    public async Task<IActionResult> ObtenerEventosPublicados()
    {
      var eventos= await _eventoRepositorio.ObtenerEventosPublicadosAsync();
      return Ok(new
      {
        total= eventos.Count,
        eventos = eventos.Select(e => new
        {
          idEvento = e.IdEvento,
          nombre = e.Nombre,
          descripcion = e.Descripcion,
          fechaHora = e.FechaHora,
          lugar = e.Lugar,
          cupoTotal = e.CupoTotal,
          estado = e.Estado
        })
      });
    }

    /*Obtenemos el detalle de un evento especifico
    GET: api/Evento/{id}*/
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerEventosPorId(int id)
    {
      var evento= await _eventoRepositorio.ObtenerPorIdAsync(id);

      if(evento==null)
        return NotFound(new {message="Evento no encontrado"});

      return Ok(new
      {
        idEvento = evento.IdEvento,
        nombre = evento.Nombre,
        descripcion = evento.Descripcion,
        fechaHora = evento.FechaHora,
        lugar = evento.Lugar,
        cupoTotal = evento.CupoTotal,
        idOrganizador = evento.IdOrganizador,
        urlPronosticoClima = evento.UrlPronosticoClima,
        datosPago = evento.DatosPago,
        estado = evento.Estado
      });
    }

    /*Endpoints para organizadores (requiere autenticacion)*/
    /*GET: api/Evento/MisEventos - Obtenemos los eventos de un organizador autenticado*/
    [Authorize]
    [HttpGet("MisEventos")]
    public async Task<IActionResult> ObtenerMisEventos()
    {
      //Obtenemos el Id del organizador desde el token
      var userIdClaimOrg= User.FindFirst(ClaimTypes.NameIdentifier);
      if(userIdClaimOrg==null)
        return Unauthorized(new {message="No autorizado"});

      var userId= int.Parse(userIdClaimOrg.Value);
      
      //Verificar que sea organizador
      var tipoUsuarioClaim= User.FindFirst("TipoUsuario");
      if(tipoUsuarioClaim == null || tipoUsuarioClaim.Value.ToLower() !="organizador")
        return BadRequest(new {message="Solo los organizadores pueden acceder a este contenido"});

      // Obtener todos los eventos del organizador
      var eventos = await _eventoRepositorio.ObtenerTodosPorOrganizadorAsync(userId);

      return Ok(new
      {
        total = eventos.Count,
        eventos = eventos.Select(e => new
        {
          idEvento = e.IdEvento,
          nombre = e.Nombre,
          descripcion = e.Descripcion,
          fechaHora = e.FechaHora,
          lugar = e.Lugar,
          cupoTotal = e.CupoTotal,
          estado = e.Estado,
          datosPago = e.DatosPago
        })
      });
    }

  /*POST: api/Nuevo - Crea un nuevo evento (Organizadores)*/
  [Authorize]
  [HttpPost]
  public async Task<IActionResult> CrearEvento ([FromBody] Evento evento)
    {
      //Validar modelo
      if(!ModelState.IsValid)
        return BadRequest (ModelState);

      //Obtener el Id organizador desde el token
      var userIdClaimOrg= User.FindFirst(ClaimTypes.NameIdentifier);
      if(userIdClaimOrg==null)
        return Unauthorized (new{message="No autorizado"});

      var userId= int.Parse(userIdClaimOrg.Value);

      //Verificar que sea organizador
      var tipoUsuarioClaim= User.FindFirst("TipoUsuario");
      if(tipoUsuarioClaim==null || tipoUsuarioClaim.Value.ToLower()!="organizador")
        return BadRequest(new{message="Solo los organizadores puede crear eventos"});

      //Asignar el token del orga desde el token
      evento.IdOrganizador=userId;

      //Validar que la fecha sea futura
      if(evento.FechaHora<DateTime.Now)
        return BadRequest(new{message="La fecha del evento debe ser futura"});

      //Crear evento
      var eventoCreado= await _eventoRepositorio.CrearEventoAsync(evento);

      return Ok(new
      {
        message="Evento creado exitosamente",
        evento= new
        {
         idEvento = eventoCreado.IdEvento,
          nombre = eventoCreado.Nombre,
          fechaHora = eventoCreado.FechaHora,
          estado = eventoCreado.Estado 
        }
      });
    }

    /*PUT: api/Evento/{id} - Actualiza un evento exitosamente (solo el organizador)*/
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarEvento(int id, [FromBody] Evento eventoActualizado)
    {
      //Validar el modelo
      if(!ModelState.IsValid)
        return BadRequest(ModelState);  

      //Obtener el iD del orga desde el token 
      var userIdClaimOrg= User.FindFirst(ClaimTypes.NameIdentifier);
      if(userIdClaimOrg==null)
        return Unauthorized(new{message="No autorizado"});

      var userId=int.Parse(userIdClaimOrg.Value);

      //Buscar el evento existente
      var evento= await _eventoRepositorio.ObtenerPorIdAsync(id);

      if(evento==null)
        return NotFound(new {message="Evento no encontrado"});

      //Validar si el evento pertenece al organizador
      if(evento.IdOrganizador !=userId)
        return Forbid(); //403 Forbidden

      // Actualizar campos (manteniendo idEvento e idOrganizador)
      evento.Nombre = eventoActualizado.Nombre;
      evento.Descripcion = eventoActualizado.Descripcion;
      evento.FechaHora = eventoActualizado.FechaHora;
      evento.Lugar = eventoActualizado.Lugar;
      evento.CupoTotal = eventoActualizado.CupoTotal;
      evento.UrlPronosticoClima = eventoActualizado.UrlPronosticoClima;
      evento.DatosPago = eventoActualizado.DatosPago;

      await _eventoRepositorio.UpdateEventoAsync(evento);

      return Ok(new
      {
        message="Evento actualizado exitosamente",
        evento= new
        {
          idEvento = evento.IdEvento,
          nombre = evento.Nombre,
          fechaHora = evento.FechaHora,
          estado = evento.Estado
        }
      });
    }

    /*PUT: api/Evento/{id}/CambiarEstado - Cambiamos el estado de un evento (publicado, cancelado, finalizado)
    Solo el orha que creo el evento puede cambiar su estado*/
    [Authorize]
    [HttpPut("{id}/CambiarEstado")]
    public async Task<IActionResult> CambiarEstadoEvento(int id, [FromBody] CambiarEstadoRequest request)
    {
  //validar modelo segun DataAnnotations
  if (!ModelState.IsValid)
    return BadRequest(ModelState);

  //obtener ID del organizador desde el token
  var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
  if (userIdClaim == null)
    return Unauthorized(new { message = "No autorizado" });

  var userId = int.Parse(userIdClaim.Value);

  //verificar que el usuario sea organizador
  var tipoUsuarioClaim = User.FindFirst("TipoUsuario");
  if (tipoUsuarioClaim == null || tipoUsuarioClaim.Value.ToLower() != "organizador")
    return BadRequest(new { message = "Solo los organizadores pueden cambiar estados de eventos" });

  //buscar el evento en la base de datos
  var evento = await _eventoRepositorio.ObtenerPorIdAsync(id);

  if (evento == null)
    return NotFound(new { message = "Evento no encontrado" });

  //VALIDACION CRITICA: Verificar que el evento pertenezca al organizador
  if (evento.IdOrganizador != userId)
    return Forbid(); // 403 Forbidden - No es tu evento

  //guardar el estado anterior para la respuesta
  var estadoAnterior = evento.Estado;

  try
  {
    //cambiar el estado del evento (validaciones de negocio en el repositorio)
    await _eventoRepositorio.CambiarEstadoEventoAsync(id, request.NuevoEstado);

    //TODO: Si es cancelacion, enviar notificacion a todos los inscriptos
    // Este codigo se implementara cuando tenga el modulo de notificaciones
    if (request.NuevoEstado.ToLower() == "cancelado" && !string.IsNullOrEmpty(request.Motivo))
    {
      // await _notificacionService.EnviarNotificacionCancelacionAsync(id, request.Motivo);
      // Log: Notificacion de cancelacion pendiente de implementar
    }

    //retornar respuesta exitosa con detalles del cambio
    return Ok(new
    {
      message = $"Estado del evento cambiado a '{request.NuevoEstado}' exitosamente",
      idEvento = id,
      nombreEvento = evento.Nombre,
      estadoAnterior,
      estadoNuevo = request.NuevoEstado,
      motivo = request.Motivo,
      fechaCambio = DateTime.Now
    });
  }
  catch (Exception ex)
  {
    //capturar errores de validacion de negocio del repositorio
    return BadRequest(new { message = ex.Message });
  }
    }


  }
}