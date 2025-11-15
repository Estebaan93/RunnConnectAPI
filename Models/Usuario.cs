//Models/Usuario
using System;
using System.ComponentModel.DataAnnotations; //Para las anotaciones y reglas de negocios
using System.ComponentModel.DataAnnotations.Schema; //Para el mapeo de la tabla

namespace RunnConnectAPI.Models
{
  [Table("Usuarios")]
  public class Usuario
  {
    [Key]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage ="El nombre es requerido")]
    [StringLength(100, MinimumLength =2, ErrorMessage ="El nombre debe tener mas de 2 caracteres")]
    public string Nombre { get; set; }= string.Empty;

    
    public string Apellido { get; set; }

    public string Email { get; set; }

    public string PasswordHast {get;set;}

    public string TipoUsuario {get;set;}

    public DateTime? FechaNacimiento {get;set;}

    [Column (TypeName ="varchar(1)")]
    public string? Genero {get;set;}

    public int? Dni {get; set;}

    public string? Agrupacion {get;set;}

    public string? TelefonoEmergencia {get;set;}

    

  }



}