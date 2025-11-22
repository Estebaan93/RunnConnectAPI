// Models/Dto/Usuario/ActualizarPerfilRunnerDto.cs
using System.ComponentModel.DataAnnotations;

namespace RunnConnectAPI.Models.Dto.Usuario
{
  /*DTO para actualizar perfil de Runner - actualiza campos en usuarios + perfiles_runners
   El avatar se actualiza con el endpoint separao PUT /api/Usuarios/Avatar*/
 
  public class ActualizarPerfilRunnerDto
  {
    /*Campos tabla usuarios*/
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength =3,ErrorMessage = "El nombre debe tener mas de 3 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El telefono es requerido")]
    [Range(1000000, 999999999, ErrorMessage = "El telefono debe tener entre 7 y 9 dígitos")]
    public int Telefono { get; set; }


    /*Campos tabla perfeiles_runners*/
    [Required(ErrorMessage ="El apellido es obligatorio")]
    [StringLength(100, MinimumLength =3,ErrorMessage = "El apellido debe tener mas de 3 caracteres")]
    public string Apellido { get; set; }=string.Empty;
    
    [Required(ErrorMessage ="La fecha de nacimiento es obligatoria")]
    public DateTime FechaNacimiento { get; set; }= DateTime.Now;

    [Required(ErrorMessage ="El genero debe ser obligatorio")]
    [StringLength(1, ErrorMessage = "El genero no puede exceder 1 caracter")]
    [RegularExpression(@"^[FMX]$", ErrorMessage = "El género debe ser F, M o X")]
    public string Genero { get; set; }= "X";
    
    [Required(ErrorMessage ="La localidad es obligatoria")]
    [StringLength(100, ErrorMessage = "La localidad no puede exceder 100 caracteres")]
    public string Localidad { get; set; }= string.Empty;

    [Required(ErrorMessage ="La agrupacion es obligatoria, si no tiene colocar 'libre' ")]
    [StringLength(100, ErrorMessage = "La agrupacion no puede exceder 100 caracteres")]
    public string Agrupacion { get; set; }=string.Empty;

    [Required(ErrorMessage ="Debe asignar un nuemro de emergencia")]
    [StringLength(50, ErrorMessage = "El telefono de emergencia no puede exceder 50 caracteres")]
    [RegularExpression(@"^\d{6,15}$", ErrorMessage = "El telefono de emergencia debe contener solo numeros entre 6 y 15 digitos")]
    public string TelefonoEmergencia { get; set; }=string.Empty;

  }
}