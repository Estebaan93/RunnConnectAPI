//Models/Inscripcion
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RunnConnectAPI.Models
{
  [Table("inscripciones")]
  public class Inscripcion
  {
    [Key]
    public int IdInscripcion {get;set;}

    [Required]
    public int IdUsuario {get;set;}

    [Required]
    public int IdCategoria {get;set;}

    public DateTime FechaInscripcion {get;set;}= DateTime.Now;

    [Required]
    [Column(TypeName="varchar(20)")]
    public string EstadoPago {get;set;}= "pendiente";
    
    [Column(TypeName="varchar(10)")]
    public string TalleRemera {get;set;} //xs, s, m , l ,xl, xxl 
    /*Ver disponibilidad del evento y ver si el evento entrega las remeras*/

    [Required]
    [Column(TypeName="tinnyint(1)")]
    public bool AceptoDeslinde {get; set;}= false;

    [StringLength(255)]
    public string? ComprobantePagoURL {get;set;}

  }
}