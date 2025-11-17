//Models/NotificacionesEvento
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RunnConnectAPI.Models
{
  [Table("notificaciones_evento")]
  public class NotificacionEvento
  {
    [Key]
    public int IdNotificacion {get;set;}

    [Required]
    public int IdEvento {get;set;}

    [Required(ErrorMessage="El titulo es requerido")]
    [StringLength(255, MinimumLength = 3, ErrorMessage = "El titulo debe tener entre 3 y 255 caracteres")]
    public string Titulo {get;set;}

    [Column(TypeName="text")]  
    public string? Mensaje {get;set;}

    public DateTime FechaEnvio {get;set;}= DateTime.Now;

  }
}