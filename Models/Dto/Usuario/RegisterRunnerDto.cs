//Models/Dto/Usuario/RegisterRunnerDto.cs
using System.ComponentModel.DataAnnotations;

namespace RunnConnectAPI.Models.Dto.Usuario
{
  /*Para el registrode runners*/
  public class RegisterRunnerDto
  {
   [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es valido")]
    [StringLength(255, ErrorMessage = "El email no puede exceder 255 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [StringLength(255, ErrorMessage = "La contraseña no puede exceder 255 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [Range(1000000, 99999999, ErrorMessage = "El DNI debe tener entre 7 y 8 dígitos")]
    public int Dni { get; set; }

    [Required(ErrorMessage = "El telefono es obligatorio")]
    [Range(1000000, 999999999, ErrorMessage = "El telefono debe tener entre 7 y 9 digitos")]
    public int Telefono { get; set; }

    // Campos opcionales especificos de Runners
    public DateTime? FechaNacimiento { get; set; }

    [StringLength(1, ErrorMessage = "El genero debe ser un solo caracter (F, M, X)")]
    public string? Genero { get; set; } // "F", "M", "X"

    [StringLength(100, ErrorMessage = "La localidad no puede exceder 100 caracteres")]
    public string? Localidad { get; set; }

    [StringLength(100, ErrorMessage = "La agrupacion no puede exceder 100 caracteres")]
    public string? Agrupacion { get; set; }

    [StringLength(50, ErrorMessage = "El telefono de emergencia no puede exceder 50 caracteres")]
    [RegularExpression(@"^\d{6,15}$", ErrorMessage = "El telefono de emergencia debe contener solo numeros (6-15 digitos)")]
    public string? TelefonoEmergencia { get; set; } 
  }


}