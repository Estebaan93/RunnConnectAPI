// Models/Dto/Usuario/ActualizarPerfilOrganizadorDto.cs
using System.ComponentModel.DataAnnotations;

namespace RunnConnectAPI.Models.Dto.Usuario
{

  //DTO para actualizar perfil de Organizador
  
  public class ActualizarPerfilOrganizadorDto
  {
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string? Apellido { get; set; }

    [Required(ErrorMessage = "El telefono es requerido")]
    [Range(1000000, 999999999, ErrorMessage = "El telefono debe tener entre 7 y 9 digitos")]
    public int Telefono { get; set; }

    [StringLength(100, ErrorMessage = "La localidad no puede exceder 100 caracteres")]
    public string? Localidad { get; set; }
  }
}