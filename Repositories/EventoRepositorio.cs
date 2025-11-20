//Repositories/EventoRepositorio.cs
using Microsoft.EntityFrameworkCore;
using RunnConnectAPI.Data;
using RunnConnectAPI.Models;

namespace RunnConnectAPI.Repositories
{
  /*Repo para gestionar eventos*/
  public class EventoRepositorio
  {
    private readonly RunnersContext _context;
    public EventoRepositorio(RunnersContext context)
    {
      _context= context;
    } 

    /*Obtiene todos los eventos organizados por un organizador (me incluye enventos futuros y pasados ultimos 6 meses) para el publico*/
    public async Task<List<Evento>> ObtenerPorOrganizadorAsync(int idOrganizador)
    {
      var fechaLimite= DateTime.Now.AddMonths(-6);

      return await _context.Eventos
        .Where(e=>e.IdOrganizador==idOrganizador &&e.FechaHora>=fechaLimite && e.Estado=="publicado"|| e.Estado=="finalizado" || e.Estado=="cancelado")
        .OrderByDescending(e=>e.FechaHora)
        .ToListAsync();  
    }

    /*Obtenemos todos los eventos del organizador - para el organizador -- verificar si el evento le pertenece al organizador*/
    public async Task<List<Evento>> ObtenerTodosPorOrganizadorAsync(int idOrganizador)
    {
      return await _context.Eventos
        .Where(e=>e.IdOrganizador==idOrganizador)
        .OrderByDescending(e=>e.FechaHora)
        .ToListAsync();
    }

    /*Obtenemos eventos publicado y futuros - para el runner busqueda*/
    public async Task<List<Evento>> ObtenerEventosPublicadosAsync()
    {
      return await _context.Eventos
        .Where(e=>e.Estado== "publicado"&&e.FechaHora>=DateTime.Now)
        .OrderBy(e=>e.FechaHora)
        .ToListAsync();
    }

    //Obtiene evento por ID
    public async Task<Evento?> ObtenerPorIdAsync(int id)
    {
      return await _context.Eventos.FindAsync(id);
    }


    /*Crea un nuevo evento - estado publicado*/
    public async Task<Evento> CrearEventoAsync(Evento evento)
    { 
      evento.Estado="publicado";

      _context.Eventos.Add(evento);
      await _context.SaveChangesAsync();
      return evento;
    }

    /*Actualizar evento existente -- verificar si el evento le pertenece al organizador en el controller*/
    public async Task UpdateEventoAsync(Evento evento)
    {
      _context.Eventos.Update(evento);
      await _context.SaveChangesAsync();
    }

    /*Cambiamos el estado de un evento - para el organizador -- verificar si el evento le pernece al organizador*/
    public async Task CambiarEstadoEventoAsync(int idEvento, string nuevoEstado)
    {
      var evento= await ObtenerPorIdAsync(idEvento);
      
      if(evento==null)
        throw new Exception("Evento no encontrado");

       //validar que el estado sea valido
       var estadosValidos = new[] { "publicado", "cancelado", "finalizado" };
      if (!estadosValidos.Contains(nuevoEstado.ToLower()))
        throw new Exception($"Estado inválido. Estados válidos: {string.Join(", ", estadosValidos)}");

      // Validaciones de lógica de negocio
      if (nuevoEstado == "finalizado" && evento.FechaHora > DateTime.Now)
        throw new Exception("No se puede finalizar un evento que aún no ha ocurrido");

      if (nuevoEstado == "publicado" && evento.FechaHora < DateTime.Now)
        throw new Exception("No se puede publicar un evento con fecha pasada");

      // Cambiar estado
      evento.Estado = nuevoEstado.ToLower();
      await UpdateEventoAsync(evento);
    }


    //Publicamos un evento
    public async Task PublicarEventoAsync(int idEvento)
    {
      await CambiarEstadoEventoAsync(idEvento,"publicado");
    }


    /*Cancelamos un evento*/
    public async Task CancelarEventoAsync(int idEvento)
    {
      await CambiarEstadoEventoAsync(idEvento, "cancelado");
    }

    /*Finalizamos un evento despues de que ocurra*/
    public async Task FinalizarEventoAsync(int idEvento)
    {
      await CambiarEstadoEventoAsync(idEvento, "finalizado");
    }


    /*Verificamos si un evento pertenece al organizador especifico*/
    public async Task<bool> PerteneceOrganizadoAsync(int idEvento, int idOrganizador)
    {
      return await _context.Eventos
        .AnyAsync(e=>e.IdEvento== idEvento && e.IdOrganizador == idOrganizador);
    }


  }
}