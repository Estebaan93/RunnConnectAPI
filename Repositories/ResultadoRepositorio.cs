// Repositories/ResultadoRepositorio.cs
using Microsoft.EntityFrameworkCore;
using RunnConnectAPI.Data;
using RunnConnectAPI.Models;
using RunnConnectAPI.Models.Dto.Resultado;

namespace RunnConnectAPI.Repositories
{
  public class ResultadoRepositorio
  {
    private readonly RunnersContext _context;

    public ResultadoRepositorio(RunnersContext context)
    {
      _context = context;
    }


    /// Obtiene un resultado por ID con toda la información relacionada
    public async Task<ResultadoResponse?> ObtenerPorIdAsync(int idResultado)
    {
      var resultado = await _context.Resultados
        .Include(r => r.Inscripcion)
          .ThenInclude(i => i!.Usuario)
            .ThenInclude(u => u!.PerfilRunner)
        .Include(r => r.Inscripcion)
          .ThenInclude(i => i!.Categoria)
            .ThenInclude(c => c!.Evento)
        .FirstOrDefaultAsync(r => r.IdResultado == idResultado);

      if (resultado == null) return null;

      return MapearAResponse(resultado);
    }

    /// Obtiene el resultado de una inscripcion especifica
  
    public async Task<ResultadoResponse?> ObtenerPorInscripcionAsync(int idInscripcion)
    {
      var resultado = await _context.Resultados
        .Include(r => r.Inscripcion)
          .ThenInclude(i => i!.Usuario)
            .ThenInclude(u => u!.PerfilRunner)
        .Include(r => r.Inscripcion)
          .ThenInclude(i => i!.Categoria)
            .ThenInclude(c => c!.Evento)
        .FirstOrDefaultAsync(r => r.IdInscripcion == idInscripcion);

      if (resultado == null) return null;

      return MapearAResponse(resultado);
    }

    /// Obtiene todos los resultados de un evento
    public async Task<ResultadosEventoResponse?> ObtenerResultadosEventoAsync(int idEvento)
    {
      var evento = await _context.Eventos
        .FirstOrDefaultAsync(e => e.IdEvento == idEvento);

      if (evento == null) return null;

      // Obtener todas las inscripciones confirmadas del evento
      var inscripcionesEvento = await _context.Inscripciones
        .Include(i => i.Usuario)
          .ThenInclude(u => u!.PerfilRunner)
        .Include(i => i.Categoria)
        .Include(i => i.Resultado)
        .Where(i => i.Categoria!.IdEvento == idEvento && i.EstadoPago == "confirmado")
        .ToListAsync();

      var resultados = inscripcionesEvento
        .Where(i => i.Resultado != null)
        .OrderBy(i => i.Resultado!.PosicionGeneral ?? int.MaxValue)
        .ThenBy(i => i.Resultado!.TiempoOficial)
        .Select(i => new ResultadoEventoItem
        {
          IdResultado = i.Resultado!.IdResultado,
          IdInscripcion = i.IdInscripcion,
          NombreRunner = $"{i.Usuario?.PerfilRunner?.Nombre} {i.Usuario?.PerfilRunner?.Apellido}".Trim(),
          DniRunner = i.Usuario?.PerfilRunner?.Dni,
          Genero = i.Usuario?.PerfilRunner?.Genero,
          Agrupacion = i.Usuario?.PerfilRunner?.Agrupacion,
          NombreCategoria = i.Categoria?.Nombre ?? "",
          TiempoOficial = i.Resultado.TiempoOficial,
          PosicionGeneral = i.Resultado.PosicionGeneral,
          PosicionCategoria = i.Resultado.PosicionCategoria,
          TieneDatosSmartwatch = !string.IsNullOrEmpty(i.Resultado.TiempoSmartwatch) ||
                                  i.Resultado.DistanciaKm.HasValue ||
                                  i.Resultado.CaloriasQuemadas.HasValue
        })
        .ToList();

      return new ResultadosEventoResponse
      {
        Evento = new EventoResultadoInfo
        {
          IdEvento = evento.IdEvento,
          Nombre = evento.Nombre,
          FechaHora = evento.FechaHora,
          Lugar = evento.Lugar,
          Estado = evento.Estado
        },
        TotalParticipantes = inscripcionesEvento.Count,
        TotalConResultado = resultados.Count,
        TotalSinResultado = inscripcionesEvento.Count - resultados.Count,
        Resultados = resultados
      };
    }

    /// Obtiene los resultados del runner autenticado
    public async Task<MisResultadosResponse> ObtenerMisResultadosAsync(int idUsuario)
    {
      var inscripciones = await _context.Inscripciones
        .Include(i => i.Categoria)
          .ThenInclude(c => c!.Evento)
        .Include(i => i.Resultado)
        .Where(i => i.IdUsuario == idUsuario && i.Resultado != null)
        .OrderByDescending(i => i.Categoria!.Evento!.FechaHora)
        .ToListAsync();

      var resultados = inscripciones.Select(i => new MiResultadoItem
      {
        IdResultado = i.Resultado!.IdResultado,
        IdInscripcion = i.IdInscripcion,
        IdEvento = i.Categoria!.IdEvento,
        NombreEvento = i.Categoria.Evento!.Nombre,
        FechaEvento = i.Categoria.Evento.FechaHora,
        LugarEvento = i.Categoria.Evento.Lugar,
        NombreCategoria = i.Categoria.Nombre,
        TiempoOficial = i.Resultado.TiempoOficial,
        PosicionGeneral = i.Resultado.PosicionGeneral,
        PosicionCategoria = i.Resultado.PosicionCategoria,
        DatosSmartwatch = new DatosSmartwatchInfo
        {
          TiempoSmartwatch = i.Resultado.TiempoSmartwatch,
          DistanciaKm = i.Resultado.DistanciaKm,
          RitmoPromedio = i.Resultado.RitmoPromedio,
          VelocidadPromedio = i.Resultado.VelocidadPromedio,
          CaloriasQuemadas = i.Resultado.CaloriasQuemadas,
          PulsacionesPromedio = i.Resultado.PulsacionesPromedio,
          PulsacionesMax = i.Resultado.PulsacionesMax
        }
      }).ToList();

      // Calcular estadísticas
      var estadisticas = new EstadisticasRunner
      {
        CarrerasCompletadas = resultados.Count,
        DistanciaTotalKm = resultados.Sum(r => r.DatosSmartwatch?.DistanciaKm ?? 0),
        CaloriasTotales = resultados.Sum(r => r.DatosSmartwatch?.CaloriasQuemadas ?? 0),
        MejorPosicion = resultados.Where(r => r.PosicionGeneral.HasValue)
                                  .Min(r => r.PosicionGeneral)
      };

      return new MisResultadosResponse
      {
        TotalCarreras = resultados.Count,
        Resultados = resultados,
        Estadisticas = estadisticas
      };
    }

    /// Obtiene resultados de un evento filtrados por categoría
    public async Task<List<ResultadoEventoItem>> ObtenerResultadosPorCategoriaAsync(int idCategoria)
    {
      var resultados = await _context.Inscripciones
        .Include(i => i.Usuario)
          .ThenInclude(u => u!.PerfilRunner)
        .Include(i => i.Categoria)
        .Include(i => i.Resultado)
        .Where(i => i.IdCategoria == idCategoria && 
                    i.EstadoPago == "confirmado" && 
                    i.Resultado != null)
        .OrderBy(i => i.Resultado!.PosicionCategoria ?? int.MaxValue)
        .ThenBy(i => i.Resultado!.TiempoOficial)
        .Select(i => new ResultadoEventoItem
        {
          IdResultado = i.Resultado!.IdResultado,
          IdInscripcion = i.IdInscripcion,
          NombreRunner = $"{i.Usuario!.PerfilRunner!.Nombre} {i.Usuario.PerfilRunner.Apellido}".Trim(),
          DniRunner = i.Usuario.PerfilRunner.Dni,
          Genero = i.Usuario.PerfilRunner.Genero,
          Agrupacion = i.Usuario.PerfilRunner.Agrupacion,
          NombreCategoria = i.Categoria!.Nombre,
          TiempoOficial = i.Resultado.TiempoOficial,
          PosicionGeneral = i.Resultado.PosicionGeneral,
          PosicionCategoria = i.Resultado.PosicionCategoria,
          TieneDatosSmartwatch = !string.IsNullOrEmpty(i.Resultado.TiempoSmartwatch)
        })
        .ToListAsync();

      return resultados;
    }

    
    // 
    /// Carga un resultado individual (Organizador)
    public async Task<(Resultado? resultado, string? error)> CargarResultadoAsync(
      CargarResultadoRequest request, int idOrganizador)
    {
      // Verificar que la inscripción existe
      var inscripcion = await _context.Inscripciones
        .Include(i => i.Categoria)
          .ThenInclude(c => c!.Evento)
        .FirstOrDefaultAsync(i => i.IdInscripcion == request.IdInscripcion);

      if (inscripcion == null)
        return (null, "La inscripción no existe");

      // Verificar que el organizador es dueño del evento
      if (inscripcion.Categoria?.Evento?.IdOrganizador != idOrganizador)
        return (null, "No tienes permiso para cargar resultados en este evento");

      // Verificar que el evento está finalizado
      if (inscripcion.Categoria?.Evento?.Estado != "finalizado")
        return (null, "Solo se pueden cargar resultados en eventos finalizados");

      // Verificar que la inscripción está confirmada
      if (inscripcion.EstadoPago != "confirmado")
        return (null, "Solo se pueden cargar resultados de inscripciones confirmadas");

      // Verificar si ya existe un resultado
      var resultadoExistente = await _context.Resultados
        .FirstOrDefaultAsync(r => r.IdInscripcion == request.IdInscripcion);

      if (resultadoExistente != null)
        return (null, "Ya existe un resultado para esta inscripción. Use el endpoint de actualización.");

      // Crear el resultado
      var resultado = new Resultado
      {
        IdInscripcion = request.IdInscripcion,
        TiempoOficial = request.TiempoOficial,
        PosicionGeneral = request.PosicionGeneral,
        PosicionCategoria = request.PosicionCategoria
      };

      _context.Resultados.Add(resultado);
      await _context.SaveChangesAsync();

      return (resultado, null);
    }

    /// Carga resultados en batch por DNI (Organizador)
    public async Task<ResultadosResponse> CargarResultadosBatchAsync(
      CargarResultadosRequest request, int idOrganizador)
    {
      var response = new ResultadosResponse
      {
        TotalProcesados = request.Resultados.Count
      };

      // Verificar que el evento existe y pertenece al organizador
      var evento = await _context.Eventos
        .FirstOrDefaultAsync(e => e.IdEvento == request.IdEvento);

      if (evento == null)
      {
        response.Errores.Add(new ResultadoError 
        { 
          Dni = 0, 
          Motivo = "El evento no existe" 
        });
        response.Fallidos = request.Resultados.Count;
        return response;
      }

      if (evento.IdOrganizador != idOrganizador)
      {
        response.Errores.Add(new ResultadoError 
        { 
          Dni = 0, 
          Motivo = "No tienes permiso para cargar resultados en este evento" 
        });
        response.Fallidos = request.Resultados.Count;
        return response;
      }

      if (evento.Estado != "finalizado")
      {
        response.Errores.Add(new ResultadoError 
        { 
          Dni = 0, 
          Motivo = "Solo se pueden cargar resultados en eventos finalizados" 
        });
        response.Fallidos = request.Resultados.Count;
        return response;
      }

      // Obtener todas las inscripciones confirmadas del evento con sus perfiles
      var inscripcionesEvento = await _context.Inscripciones
        .Include(i => i.Usuario)
          .ThenInclude(u => u!.PerfilRunner)
        .Include(i => i.Categoria)
        .Include(i => i.Resultado)
        .Where(i => i.Categoria!.IdEvento == request.IdEvento && 
                    i.EstadoPago == "confirmado")
        .ToListAsync();

      foreach (var item in request.Resultados)
      {
        // Buscar inscripción por DNI
        var inscripcion = inscripcionesEvento
          .FirstOrDefault(i => i.Usuario?.PerfilRunner?.Dni == item.Dni);

        if (inscripcion == null)
        {
          response.Errores.Add(new ResultadoError
          {
            Dni = item.Dni,
            Motivo = "No se encontró inscripción confirmada con este DNI"
          });
          response.Fallidos++;
          continue;
        }

        // Verificar si ya tiene resultado
        if (inscripcion.Resultado != null)
        {
          // Actualizar existente
          inscripcion.Resultado.TiempoOficial = item.TiempoOficial;
          inscripcion.Resultado.PosicionGeneral = item.PosicionGeneral;
          inscripcion.Resultado.PosicionCategoria = item.PosicionCategoria;
        }
        else
        {
          // Crear nuevo
          var nuevoResultado = new Resultado
          {
            IdInscripcion = inscripcion.IdInscripcion,
            TiempoOficial = item.TiempoOficial,
            PosicionGeneral = item.PosicionGeneral,
            PosicionCategoria = item.PosicionCategoria
          };
          _context.Resultados.Add(nuevoResultado);
        }

        response.Exitosos++;
      }

      await _context.SaveChangesAsync();
      return response;
    }

    /// Actualiza el tiempo oficial de un resultado (Organizador)
    public async Task<(bool exito, string? error)> ActualizarTiempoOficialAsync(
      int idResultado, string tiempoOficial, int idOrganizador)
    {
      var resultado = await _context.Resultados
        .Include(r => r.Inscripcion)
          .ThenInclude(i => i!.Categoria)
            .ThenInclude(c => c!.Evento)
        .FirstOrDefaultAsync(r => r.IdResultado == idResultado);

      if (resultado == null)
        return (false, "El resultado no existe");

      if (resultado.Inscripcion?.Categoria?.Evento?.IdOrganizador != idOrganizador)
        return (false, "No tienes permiso para modificar este resultado");

      resultado.TiempoOficial = tiempoOficial;
      await _context.SaveChangesAsync();

      return (true, null);
    }

    /// Actualiza las posiciones de un resultado (Organizador)
    public async Task<(bool exito, string? error)> ActualizarPosicionesAsync(
      int idResultado, ActualizarPosicionesRequest request, int idOrganizador)
    {
      var resultado = await _context.Resultados
        .Include(r => r.Inscripcion)
          .ThenInclude(i => i!.Categoria)
            .ThenInclude(c => c!.Evento)
        .FirstOrDefaultAsync(r => r.IdResultado == idResultado);

      if (resultado == null)
        return (false, "El resultado no existe");

      if (resultado.Inscripcion?.Categoria?.Evento?.IdOrganizador != idOrganizador)
        return (false, "No tienes permiso para modificar este resultado");

      if (request.PosicionGeneral.HasValue)
        resultado.PosicionGeneral = request.PosicionGeneral;

      if (request.PosicionCategoria.HasValue)
        resultado.PosicionCategoria = request.PosicionCategoria;

      await _context.SaveChangesAsync();
      return (true, null);
    }

    /// Agrega datos de smartwatch a un resultado (Runner)
    public async Task<(bool exito, string? error)> AgregarDatosSmartwatchAsync(
      int idResultado, DatosSmartwatchRequest request, int idUsuario)
    {
      var resultado = await _context.Resultados
        .Include(r => r.Inscripcion)
        .FirstOrDefaultAsync(r => r.IdResultado == idResultado);

      if (resultado == null)
        return (false, "El resultado no existe");

      // Verificar que el resultado pertenece al usuario
      if (resultado.Inscripcion?.IdUsuario != idUsuario)
        return (false, "No tienes permiso para modificar este resultado");

      // Actualizar datos de smartwatch
      if (request.TiempoSmartwatch != null)
        resultado.TiempoSmartwatch = request.TiempoSmartwatch;
      
      if (request.DistanciaKm.HasValue)
        resultado.DistanciaKm = request.DistanciaKm;
      
      if (request.RitmoPromedio != null)
        resultado.RitmoPromedio = request.RitmoPromedio;
      
      if (request.VelocidadPromedio != null)
        resultado.VelocidadPromedio = request.VelocidadPromedio;
      
      if (request.CaloriasQuemadas.HasValue)
        resultado.CaloriasQuemadas = request.CaloriasQuemadas;
      
      if (request.PulsacionesPromedio.HasValue)
        resultado.PulsacionesPromedio = request.PulsacionesPromedio;
      
      if (request.PulsacionesMax.HasValue)
        resultado.PulsacionesMax = request.PulsacionesMax;

      await _context.SaveChangesAsync();
      return (true, null);
    }

    /// Elimina un resultado (Organizador)
    public async Task<(bool exito, string? error)> EliminarResultadoAsync(
      int idResultado, int idOrganizador)
    {
      var resultado = await _context.Resultados
        .Include(r => r.Inscripcion)
          .ThenInclude(i => i!.Categoria)
            .ThenInclude(c => c!.Evento)
        .FirstOrDefaultAsync(r => r.IdResultado == idResultado);

      if (resultado == null)
        return (false, "El resultado no existe");

      if (resultado.Inscripcion?.Categoria?.Evento?.IdOrganizador != idOrganizador)
        return (false, "No tienes permiso para eliminar este resultado");

      _context.Resultados.Remove(resultado);
      await _context.SaveChangesAsync();

      return (true, null);
    }

  
    // ═══════════════════ HELPERS ═══════════════════

    /// Verifica si existe un resultado para una inscripción
    public async Task<bool> ExisteResultadoParaInscripcionAsync(int idInscripcion)
    {
      return await _context.Resultados
        .AnyAsync(r => r.IdInscripcion == idInscripcion);
    }

    /// Verifica si el usuario es el dueño de un resultado
    public async Task<bool> EsDuenioDelResultadoAsync(int idResultado, int idUsuario)
    {
      return await _context.Resultados
        .Include(r => r.Inscripcion)
        .AnyAsync(r => r.IdResultado == idResultado && 
                       r.Inscripcion!.IdUsuario == idUsuario);
    }

    private ResultadoResponse MapearAResponse(Resultado resultado)
    {
      return new ResultadoResponse
      {
        IdResultado = resultado.IdResultado,
        IdInscripcion = resultado.IdInscripcion,
        Runner = resultado.Inscripcion?.Usuario?.PerfilRunner != null
          ? new RunnerResultadoInfo
          {
            IdUsuario = resultado.Inscripcion.Usuario.IdUsuario,
            NombreCompleto = $"{resultado.Inscripcion.Usuario.PerfilRunner.Nombre} {resultado.Inscripcion.Usuario.PerfilRunner.Apellido}".Trim(),
            Dni = resultado.Inscripcion.Usuario.PerfilRunner.Dni,
            Genero = resultado.Inscripcion.Usuario.PerfilRunner.Genero,
            Agrupacion = resultado.Inscripcion.Usuario.PerfilRunner.Agrupacion
          }
          : null,
        Categoria = resultado.Inscripcion?.Categoria != null
          ? new CategoriaResultadoInfo
          {
            IdCategoria = resultado.Inscripcion.Categoria.IdCategoria,
            Nombre = resultado.Inscripcion.Categoria.Nombre,
            IdEvento = resultado.Inscripcion.Categoria.IdEvento,
            NombreEvento = resultado.Inscripcion.Categoria.Evento?.Nombre ?? ""
          }
          : null,
        TiempoOficial = resultado.TiempoOficial,
        PosicionGeneral = resultado.PosicionGeneral,
        PosicionCategoria = resultado.PosicionCategoria,
        DatosSmartwatch = new DatosSmartwatchInfo
        {
          TiempoSmartwatch = resultado.TiempoSmartwatch,
          DistanciaKm = resultado.DistanciaKm,
          RitmoPromedio = resultado.RitmoPromedio,
          VelocidadPromedio = resultado.VelocidadPromedio,
          CaloriasQuemadas = resultado.CaloriasQuemadas,
          PulsacionesPromedio = resultado.PulsacionesPromedio,
          PulsacionesMax = resultado.PulsacionesMax
        }
      };
    }

  }
}