// Models/Dto/Usuario/ActualizarPerfilRunnerDto.cs
using System.ComponentModel.DataAnnotations;

namespace RunnConnectAPI.Models.Dto.Usuario
{
  //DTO para actualizar perfil de Runner
 
  public class ActualizarPerfilRunnerDto
  {
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string? Apellido { get; set; }

    [Required(ErrorMessage = "El telefono es requerido")]
    [Range(1000000, 999999999, ErrorMessage = "El telefono debe tener entre 7 y 9 dígitos")]
    public int Telefono { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    [StringLength(1, ErrorMessage = "El genero no puede exceder 1 caracter")]
    public string? Genero { get; set; }

    [StringLength(100, ErrorMessage = "La localidad no puede exceder 100 caracteres")]
    public string? Localidad { get; set; }

    [StringLength(100, ErrorMessage = "La agrupacion no puede exceder 100 caracteres")]
    public string? Agrupacion { get; set; }

    [StringLength(50, ErrorMessage = "El telefono de emergencia no puede exceder 50 caracteres")]
    [RegularExpression(@"^\d{6,15}$", ErrorMessage = "El telefono de emergencia debe contener solo numeros entre 6 y 15 digitos")]
    public string? TelefonoEmergencia { get; set; }
  }
}