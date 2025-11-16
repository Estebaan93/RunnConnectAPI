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


  }
}