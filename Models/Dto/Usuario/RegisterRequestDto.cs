//Models/Dto/Usuario/RegisterRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace RunnConnectAPI.Models.Dto.Usuario
{
  public class RegisterRequestDto
  {
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es valido")]
    [StringLength(255, ErrorMessage = "El email no puede exceder 255 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [StringLength(255, ErrorMessage = "La contraseña no puede exceder 255 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "La confirmacion de contraseña es obligatoria")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [StringLength(20, ErrorMessage = "El DNI no puede exceder 20 caracteres")]
    public string Dni { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El formato del telefono no es valido")]
    [StringLength(20, ErrorMessage = "El telefono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    [StringLength(10, ErrorMessage = "El genero no puede exceder 10 caracteres")]
    public string? Genero { get; set; }

    [StringLength(255, ErrorMessage = "La ciudad no puede exceder 255 caracteres")]
    public string? Ciudad { get; set; }

    // Por defecto será "Participante"
    public string TipoUsuario { get; set; } = "Participante";
  }
}