// Models/Dto/Usuario/ActualizarPerfilOrganizadorDto.cs
using System.ComponentModel.DataAnnotations;

namespace RunnConnectAPI.Models.Dto.Usuario
{

  /*DTO para actualizar perfil de Organizador - actualiza campos en usuarios + perfiles_organizadoes
  El avatar se actualiza con el endpoint separado PUT /api/Usuarios/Avatar*/
  
  public class ActualizarPerfilOrganizadorDto
  {
    /*Campos tabla usuarios*/
    [Required(ErrorMessage = "El nombre comercial es obligatorio")]
    [StringLength(100, MinimumLength =3, ErrorMessage = "El nombre debe tener mas de 3 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El telefono es requerido")]
    [Range(1000000, 999999999, ErrorMessage = "El telefono debe tener entre 7 y 9 digitos")]
    public int Telefono { get; set; }


    /*Campos tabla perfiles_organizadors*/
    [Required(ErrorMessage = "La razón social es obligatoria")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "La razón social debe tener entre 2 y 100 caracteres")]
    public string RazonSocial { get; set; } = string.Empty;

    [Required(ErrorMessage ="El nombre comercial es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre comercial no puede exceder 100 caracteres")]
    public string NombreComercial { get; set; }=string.Empty;

    [Required(ErrorMessage ="La direccion es obligatoria")]
    [StringLength(255, ErrorMessage = "La dirección legal no puede exceder 255 caracteres")]
    public string DireccionLegal { get; set; }=string.Empty;

  }
}