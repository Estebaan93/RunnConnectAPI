// Models/Dto/Usuario/RegisterOrganizadorDto.cs

using System.ComponentModel.DataAnnotations;

namespace RunnConnectAPI.Models.Dto.Usuario
{
  /*DTO para registro de organizadores*/
public class RegisterOrganizadorDto
  {
    [Required(ErrorMessage = "El nombre de la organizacion es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido/razon social es obligatorio")]
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

    [Required(ErrorMessage = "La confirmacion de contraseña es obligatoria")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "El telefono es obligatorio")]
    [Range(1000000, 999999999, ErrorMessage = "El telefono debe tener entre 7 y 9 dígitos")]
    public int Telefono { get; set; }

    // Campo opcional para Organizadores
    [StringLength(100, ErrorMessage = "La localidad no puede exceder 100 caracteres")]
    public string? Localidad { get; set; }
  }

}