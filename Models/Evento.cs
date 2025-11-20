//Models/Evento
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RunnConnectAPI.Models
{
  [Table("eventos")]
  public class Evento
  {
    [Key]
    public int IdEvento {get;set;}

    [Required(ErrorMessage ="El nombre del evento es requerido")]
    [StringLength(255, MinimumLength =3, ErrorMessage ="El nombre debe tener mas 3 caracteres")]
    public string Nombre {get;set;}= string.Empty;

    [Column(TypeName ="text")]
    public string? Descripcion {get;set;}

    [Required(ErrorMessage ="La fecha y hora del evento son requeridss")]
    public DateTime FechaHora {get;set;}

    [Required(ErrorMessage ="El lugar es requerido")]
    [StringLength(255)]
    public string Lugar {get;set;}= string.Empty;

    public int? CupoTotal {get;set;}

    [Required]
    public int IdOrganizador {get;set;}

    [StringLength(255)]
    public string? UrlPronosticoClima {get;set;}

    [Required(ErrorMessage ="Los datos para el pago son obligatorios")]
    [Column(TypeName ="text")]
    public string DatosPago {get;set;}= string.Empty;

    [Required]
    [Column(TypeName ="varchar(20)")]
    public string Estado{get;set;}= "publicado";

  }
}