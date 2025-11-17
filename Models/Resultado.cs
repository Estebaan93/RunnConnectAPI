//Models/Resultado.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RunnConnectAPI.Models
{
  [Table("resultado")]
  public class Resultado
  {
   [Key]
   public int IdResultado {get;set;}   

   [Required]
   public int IdInscripcion{get;set;} 

   //Estos campos se cargan por el organizador (post-evento)
   [StringLength(20)]
   [RegularExpression(@"^\d{2}:\d{2}:\d{2}(\.\d{1,3})?$", ErrorMessage ="El formato de tiempo es invalido (HH:MM:SS.mm.)")]
   public string? TiempoOficial {get;set;} 

  [Range(1,10000, ErrorMessage ="La posicion general debe ser mayor a 0")]
   public int? PosicionGeneral {get;set;}

  [Range(1,10000, ErrorMessage ="La posicion categoria debe ser mayor a 0")]
   public int? PosicionCategoria {get;set;}


  //Datos anexados por el runner (con su smartwatch)
  [StringLength(20)]
  [RegularExpression(@"^\d{2}:\d{2}:\d{2}(\.\d{1,3})?$", ErrorMessage = "Formato de tiempo invalido (HH:MM:SS.mmm)")]
   public string? TiempoSmartWatch {get;set;}

  [Column(TypeName = "decimal(6,2)")]
  [Range(0, 999.99, ErrorMessage = "La distancia debe estar entre 0 y 999.99 km")]
   public decimal? DistanciaKm {get;set;}

  [StringLength(20)]
   public string? RitmoPromedio {get;set;}     

  [StringLength(20)]
   public string? VelocidadPromedio {get;set;}

  [Range(0, 10000, ErrorMessage = "Las calorias deben estar entre 0 y 10000")]
   public int? CaloriasQuemadas {get;set;}

  [Range(30, 250, ErrorMessage = "Las pulsaciones promedio deben estar entre 30 y 250")]
   public int? PulsacionesPromedio {get;set;}

  [Range(30, 250, ErrorMessage = "Las pulsaciones maximas deben estar entre 30 y 250")]
   public int? PulsacionesMax {get;set;} 



  
  }
}