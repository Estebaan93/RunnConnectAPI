//Models/Usuario
using System;
using System.ComponentModel.DataAnnotations; //Para las anotaciones y reglas de negocios
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; //Para el mapeo de la tabla

namespace RunnConnectAPI.Models
{
  [Table("Usuarios")]
  public class Usuario
  {
    [Key]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener mas de 2 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe contener mas de 2 caracteres")]
    public string Apellido { get; set; }

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es valido")]
    public string Email { get; set; }

    [Required(ErrorMessage ="El telefono es requerido")]
    [Range(1000000, 999999999, ErrorMessage = "El telefono debe tener entre 7 y 9 dígitos")]
    [Column("telefono")]
    public int Telefono { get; set; }

    [Required]
    [JsonIgnore] //No exponer el hash de password en las respuestas
    public string PasswordHash { get; set; }

    [Required(ErrorMessage = "El tipo de usuario es requerido")]
    [Column(TypeName = "varchar(20)")]
    public string TipoUsuario { get; set; } = string.Empty; //"runner" o "organizador"

    public DateTime? FechaNacimiento { get; set; }

    [Column(TypeName = "varchar(1)")]
    public string? Genero { get; set; } //"F", "M" o "X"

    [Range(1000000, 99999999, ErrorMessage = "El DNI debe tener entre 7 y 8 digitos")]
    public int? Dni { get; set; }

    [StringLength(100)]
    public string? Localidad { get; set; }

    [StringLength(100)]
    public string? Agrupacion { get; set; }

    [StringLength(50)]
    [RegularExpression(@"^\d{6,15}$", ErrorMessage = "El telefono debe contener solo numeros")]
    public string? TelefonoEmergencia { get; set; }

    //Estado para borrado logico
    [Column("estado", TypeName = "tinyint(1)")]
    public bool Estado { get; set; } = true;

  }
}