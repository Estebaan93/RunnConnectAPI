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

   //Estos campos se cargan por el organizador 
   public string? TiempoOficial {get;set;} 

   public int? PosicionCategoria {get;set;}

   public string? TiempoSmartWatch {get;set;}

   public decimal? DistanciaKm {get;set;}

   public string? RitmoPromedio {get;set;}     

   public string? VelocidadPromedio {get;set;}

   public int? CaloriasQuemadas {get;set;}

   public int? PulsacionesPromedio {get;set;}

   public int? PulsacionesMax {get;set;} 



  
  }
}