// Models/Dto/Usuario/RegisterOrganizadorDto.cs

using System.ComponentModel.DataAnnotations;

namespace RunnConnectAPI.Models.Dto.Usuario
{
  /*DTO para registro de organizadores - se crean registros en usuarios + perfiles_organizadores*/
public class RegisterOrganizadorDto
  {
    //Campos tabla usuarios (comun)
    [Required(ErrorMessage = "El nombre de la organizacion es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es valido")]
    [StringLength(50, ErrorMessage = "El email no puede exceder 50 caracteres")]
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


    /*Campos tabla perfiles_organizadores*/

    [Required(ErrorMessage = "La razon social es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "La razon social debe tener entre 2 y 100 caracteres")]
    public string RazonSocial { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El nombre comercial no puede exceder 100 caracteres")]
    public string? NombreComercial { get; set; }

    [Required(ErrorMessage = "El CUIT es obligatorio")]
    [StringLength(30, MinimumLength = 11, ErrorMessage = "El CUIT debe tener entre 11 y 30 caracteres")]
    [RegularExpression(@"^\d{2}-\d{8}-\d{1}$|^\d{11}$", ErrorMessage = "El CUIT debe tener formato XX-XXXXXXXX-X o 11 dígitos")]
    public string CuitTaxId { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La direccion legal no puede exceder 255 caracteres")]
    public string? DireccionLegal { get; set; }


    /*Avatar opcional - Se envia como archivo en multipart/Form-data
    Si no se envia se asigna uno por defecto*/
    public IFormFile? ImgAvatar {get; set;}

  }

}