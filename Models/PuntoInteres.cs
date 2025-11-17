//Models/PuntoInteres
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RunnConnectAPI.Models
{
  [Table("puntosinteres")]
  public class PuntoInteres
  {
    [Key]
    public int IdPuntoInteres {get;set;}

    [Required]
    public int IdEvento {get;set;}

    [Required]
    [Column(TypeName="varchar(50)")]
    public string Tipo {get;set;} = string.Empty; //Hidratacion, meta, largada

    [Required(ErrorMessage ="El nombre del punto de interes es obligatorio")]
    [StringLength(100)]
    public string Nombre {get;set;} = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10,7)")]
    [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90")]
    public decimal Latitud { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,7)")]
    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180")]
    public decimal Longitud { get; set; }

  }
}