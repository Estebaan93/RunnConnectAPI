using Microsoft.EntityFrameworkCore;
using RunnConnectAPI.Models;

namespace RunnConnectAPI.Data
{
    public class RunnersContext : DbContext
    {
        public RunnersContext(DbContextOptions<RunnersContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<CategoriaEvento> CategoriasEvento { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<Resultado> Resultados { get; set; }
        public DbSet<Ruta> Rutas { get; set; }
        public DbSet<PuntoInteres> PuntosInteres { get; set; }
        public DbSet<NotificacionEvento> NotificacionesEvento { get; set; }
    }
}