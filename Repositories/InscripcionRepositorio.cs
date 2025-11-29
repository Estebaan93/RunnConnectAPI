//Repositories/InscripcionRepositorio.cs

using Microsoft.EntityFrameworkCore;
using RunnConnectAPI.Data;
using RunnConnectAPI.Models;

namespace RunnConnectAPI.Repositories
{

  // Repositorio para gestion de inscripciones

  public class InscripcionRepositorio
  {
    private readonly RunnersContext _context;

    public InscripcionRepositorio(RunnersContext context)
    {
      _context = context;
    }

    // Consultas para Runners
    // Obtiene todas las inscripciones de un runner
    public async Task<List<Inscripcion>> ObtenerPorRunnerAsync(int idUsuario)
    {
      return await _context.Inscripciones
          .Include(i => i.Categoria)
              .ThenInclude(c => c!.Evento)
          .Where(i => i.IdUsuario == idUsuario)
          .OrderByDescending(i => i.FechaInscripcion)
          .ToListAsync();
    }

    // Obtiene inscripciones activas (pendiente o confirmado) de un runner
    public async Task<List<Inscripcion>> ObtenerActivasPorRunnerAsync(int idUsuario)
    {
      var estadosActivos = new[] { "pendiente", "confirmado" };

      return await _context.Inscripciones
          .Include(i => i.Categoria)
              .ThenInclude(c => c!.Evento)
          .Where(i => i.IdUsuario == idUsuario && estadosActivos.Contains(i.EstadoPago))
          .OrderByDescending(i => i.FechaInscripcion)
          .ToListAsync();
    }

    // Obtiene una inscripción por ID con todas sus relaciones
    public async Task<Inscripcion?> ObtenerPorIdAsync(int idInscripcion)
    {
      return await _context.Inscripciones
          .Include(i => i.Usuario)
              .ThenInclude(u => u!.PerfilRunner)
          .Include(i => i.Categoria)
              .ThenInclude(c => c!.Evento)
          .FirstOrDefaultAsync(i => i.IdInscripcion == idInscripcion);
    }

 
    // Verifica si un runner ya esta inscripto en una categoria
    public async Task<bool> ExisteInscripcionAsync(int idUsuario, int idCategoria)
    {
      var estadosActivos = new[] { "pendiente", "confirmado" };

      return await _context.Inscripciones
          .AnyAsync(i => i.IdUsuario == idUsuario
              && i.IdCategoria == idCategoria
              && estadosActivos.Contains(i.EstadoPago));
    }


    /// Verifica si un runner ya esta inscripto en el mismo evento (cualquier categoria)
    public async Task<bool> ExisteInscripcionEnEventoAsync(int idUsuario, int idEvento)
    {
      var estadosActivos = new[] { "pendiente", "confirmado" };

      return await _context.Inscripciones
          .Include(i => i.Categoria)
          .AnyAsync(i => i.IdUsuario == idUsuario
              && i.Categoria != null
              && i.Categoria.IdEvento == idEvento
              && estadosActivos.Contains(i.EstadoPago));
    }

    // Consultas para Organizadores
    // Obtiene todas las inscripciones de un evento
    public async Task<List<Inscripcion>> ObtenerPorEventoAsync(int idEvento)
    {
      return await _context.Inscripciones
          .Include(i => i.Usuario)
              .ThenInclude(u => u!.PerfilRunner)
          .Include(i => i.Categoria)
          .Where(i => i.Categoria != null && i.Categoria.IdEvento == idEvento)
          .OrderByDescending(i => i.FechaInscripcion)
          .ToListAsync();
    }

    /// Obtiene inscripciones de un evento con filtros y paginacion
    public async Task<(List<Inscripcion> inscripciones, int totalCount)> ObtenerPorEventoConFiltrosAsync(
        int idEvento,
        int? idCategoria = null,
        string? estadoPago = null,
        string? buscarRunner = null,
        int pagina = 1,
        int tamanioPagina = 20)
    {
      var query = _context.Inscripciones
          .Include(i => i.Usuario)
              .ThenInclude(u => u!.PerfilRunner)
          .Include(i => i.Categoria)
          .Where(i => i.Categoria != null && i.Categoria.IdEvento == idEvento)
          .AsQueryable();

      // Filtrar por categoría
      if (idCategoria.HasValue)
        query = query.Where(i => i.IdCategoria == idCategoria.Value);

      // Filtrar por estado de pago
      if (!string.IsNullOrWhiteSpace(estadoPago))
        query = query.Where(i => i.EstadoPago == estadoPago.ToLower().Trim());

      // Buscar por nombre/apellido del runner
      if (!string.IsNullOrWhiteSpace(buscarRunner))
      {
        var busqueda = buscarRunner.ToLower().Trim();
        query = query.Where(i => i.Usuario != null &&
            (i.Usuario.Nombre.ToLower().Contains(busqueda) ||
             (i.Usuario.PerfilRunner != null &&
              (i.Usuario.PerfilRunner.Nombre.ToLower().Contains(busqueda) ||
               i.Usuario.PerfilRunner.Apellido.ToLower().Contains(busqueda)))));
      }

      // Contar total antes de paginar
      var totalCount = await query.CountAsync();

      // Aplicar paginacion
      var inscripciones = await query
          .OrderByDescending(i => i.FechaInscripcion)
          .Skip((pagina - 1) * tamanioPagina)
          .Take(tamanioPagina)
          .ToListAsync();

      return (inscripciones, totalCount);
    }


    // Obtiene inscripciones de una categoría especifica
    public async Task<List<Inscripcion>> ObtenerPorCategoriaAsync(int idCategoria)
    {
      return await _context.Inscripciones
          .Include(i => i.Usuario)
              .ThenInclude(u => u!.PerfilRunner)
          .Where(i => i.IdCategoria == idCategoria)
          .OrderByDescending(i => i.FechaInscripcion)
          .ToListAsync();
    }


    // Operaciones CRUD
    // Crea una nueva inscripcion
    public async Task<Inscripcion> CrearAsync(Inscripcion inscripcion)
    {
      inscripcion.FechaInscripcion = DateTime.Now;
      inscripcion.EstadoPago = "pendiente";
      inscripcion.TalleRemera = inscripcion.TalleRemera?.ToUpper().Trim();

      _context.Inscripciones.Add(inscripcion);
      await _context.SaveChangesAsync();

      return inscripcion;
    }

    /// Actualiza una inscripcion existente
    public async Task ActualizarAsync(Inscripcion inscripcion)
    {
      inscripcion.TalleRemera = inscripcion.TalleRemera?.ToUpper().Trim();
      
      _context.Inscripciones.Update(inscripcion);
      await _context.SaveChangesAsync();
    }

    /// Cambia el estado de pago de una inscripcion
    public async Task CambiarEstadoPagoAsync(int idInscripcion, string nuevoEstado)
    {
      var inscripcion = await _context.Inscripciones.FindAsync(idInscripcion);

      if (inscripcion == null)
        throw new KeyNotFoundException("Inscripción no encontrada");

      nuevoEstado = nuevoEstado.ToLower().Trim();

      var estadosValidos = new[] { "pendiente", "confirmado", "rechazado", "cancelado", "reembolsado" };
      if (!estadosValidos.Contains(nuevoEstado))
        throw new ArgumentException($"Estado inválido. Estados válidos: {string.Join(", ", estadosValidos)}");

      // Validaciones de transicion
      ValidarTransicionEstado(inscripcion.EstadoPago, nuevoEstado);

      inscripcion.EstadoPago = nuevoEstado;
      await _context.SaveChangesAsync();
    }

    /// Actualiza la URL del comprobante de pago
    public async Task ActualizarComprobanteAsync(int idInscripcion, string urlComprobante)
    {
      var inscripcion = await _context.Inscripciones.FindAsync(idInscripcion);

      if (inscripcion == null)
        throw new KeyNotFoundException("Inscripción no encontrada");

      inscripcion.ComprobantePagoURL = urlComprobante;
      await _context.SaveChangesAsync();
    }


    // Validaciones
    // Valida las reglas de transicion de estado
    private void ValidarTransicionEstado(string estadoActual, string nuevoEstado)
    {
      // No se puede modificar una inscripcion cancelada
      if (estadoActual == "cancelado")
        throw new InvalidOperationException("No se puede modificar una inscripción cancelada");

      // No se puede modificar una inscripcion reembolsada
      if (estadoActual == "reembolsado")
        throw new InvalidOperationException("No se puede modificar una inscripción reembolsada");

      // Solo se puede cancelar si esta pendiente
      if (nuevoEstado == "cancelado" && estadoActual != "pendiente")
        throw new InvalidOperationException("Solo se pueden cancelar inscripciones pendientes");

      // Solo se puede reembolsar si está confirmado
      if (nuevoEstado == "reembolsado" && estadoActual != "confirmado")
        throw new InvalidOperationException("Solo se pueden reembolsar inscripciones confirmadas");
    }

    /// Verifica si el runner cumple con los requisitos de la categoria (edad y género)
    public async Task<(bool cumple, string? motivo)> ValidarRequisitosCategoria(int idUsuario, int idCategoria)
    {
      var usuario = await _context.Usuarios
          .Include(u => u.PerfilRunner)
          .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

      var categoria = await _context.CategoriasEvento.FindAsync(idCategoria);

      if (usuario == null || categoria == null)
        return (false, "Usuario o categoría no encontrada");

      if (usuario.PerfilRunner == null)
        return (false, "Debe completar su perfil de runner");

      // Validar fecha de nacimiento
      if (!usuario.PerfilRunner.FechaNacimiento.HasValue)
        return (false, "Debe completar su fecha de nacimiento en el perfil");

      // Calcular edad
      var edad = (int)((DateTime.Now - usuario.PerfilRunner.FechaNacimiento.Value).TotalDays / 365.25);

      // Validar rango de edad
      if (edad < categoria.EdadMinima || edad > categoria.EdadMaxima)
        return (false, $"Su edad ({edad} años) no está dentro del rango permitido ({categoria.EdadMinima}-{categoria.EdadMaxima} años)");

      // Validar genero (si la categoria no es mixta)
      if (categoria.Genero != "X")
      {
        if (string.IsNullOrEmpty(usuario.PerfilRunner.Genero))
          return (false, "Debe completar su género en el perfil");

        if (usuario.PerfilRunner.Genero != categoria.Genero)
          return (false, $"Esta categoría es solo para género {(categoria.Genero == "F" ? "Femenino" : "Masculino")}");
      }

      return (true, null);
    }

    /// Verifica si el runner tiene perfil completo para inscribirse
    public async Task<(bool completo, List<string> camposFaltantes)> ValidarPerfilCompletoRunner(int idUsuario)
    {
      var usuario = await _context.Usuarios
          .Include(u => u.PerfilRunner)
          .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

      var faltantes = new List<string>();

      if (usuario == null)
      {
        faltantes.Add("Usuario no encontrado");
        return (false, faltantes);
      }

      if (usuario.PerfilRunner == null)
      {
        faltantes.Add("Perfil de runner");
        return (false, faltantes);
      }

      var perfil = usuario.PerfilRunner;

      if (string.IsNullOrEmpty(perfil.Nombre))
        faltantes.Add("Nombre");
      if (string.IsNullOrEmpty(perfil.Apellido))
        faltantes.Add("Apellido");
      if (!perfil.FechaNacimiento.HasValue)
        faltantes.Add("Fecha de nacimiento");
      if (string.IsNullOrEmpty(perfil.Genero))
        faltantes.Add("Género");
      if (!perfil.Dni.HasValue)
        faltantes.Add("DNI");
      if (string.IsNullOrEmpty(perfil.Localidad))
        faltantes.Add("Localidad");
      if (string.IsNullOrEmpty(perfil.NombreContactoEmergencia))
        faltantes.Add("Contacto de emergencia");
      if (string.IsNullOrEmpty(perfil.TelefonoEmergencia))
        faltantes.Add("Teléfono de emergencia");
      if (string.IsNullOrEmpty(usuario.Telefono))
        faltantes.Add("Teléfono");

      return (faltantes.Count == 0, faltantes);
    }

  
    // Estadisticas

    /// Cuenta inscripciones por estado en un evento
    public async Task<Dictionary<string, int>> ContarPorEstadoEnEventoAsync(int idEvento)
    {
      var inscripciones = await _context.Inscripciones
          .Include(i => i.Categoria)
          .Where(i => i.Categoria != null && i.Categoria.IdEvento == idEvento)
          .GroupBy(i => i.EstadoPago)
          .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
          .ToListAsync();

      return inscripciones.ToDictionary(x => x.Estado, x => x.Cantidad);
    }

    /// Cuenta inscripciones confirmadas en un evento
    public async Task<int> ContarConfirmadosEnEventoAsync(int idEvento)
    {
      return await _context.Inscripciones
          .Include(i => i.Categoria)
          .CountAsync(i => i.Categoria != null
              && i.Categoria.IdEvento == idEvento
              && i.EstadoPago == "confirmado");
    }

  }  
}