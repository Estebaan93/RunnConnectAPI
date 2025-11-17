//Models/Ruta.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RunnConnectAPI.Models
{
  [Table("rutas")]
  public class Ruta
  {
    [Key]
    public int IdRuta{get;set;}

    [Required]
    public int IdEvento {get;set;}

    [Required]
    [Range(1,10000, ErrorMessage="El orden debe ser mayor a 0")] //Orden del los puntos de trazado 1,2,4 etc
    public int Orden {get;set;}

    [Required]
    [Column(TypeName="decimal(10,7)")]
    [Range(-90,90, ErrorMessage ="La latidu debe estar entre -90 y 90")]
    public decimal Latitud{get;set;}

    [Required]
    [Column(TypeName = "decimal(10,7)")]
    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180")]
    public decimal Longitud{get;set;} 

  

  }
}