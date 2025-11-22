//Models/PerfilRunner.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RunnConnectAPI.Models
{
  /*Perfil especifico para runners- Datos personales y deportivos
  Relacion 1:1 con Usuario*/
  [Table("perfiles_runners")]
  public class PerfilRunner
  {
    [Key]
    [Column("idPerfilRunner")]
    public int idPerfilRunner {get;set;}

    [Required]
    [Column("idUsuario")]
    public int IdUsuario {get;set;}  

[Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100, MinimumLength = 2)]
    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    [Column("fechaNacimiento", TypeName = "date")]
    public DateTime FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El genero es obligatorio")]
    [Column("genero", TypeName = "enum('F','M','X')")]
    public string Genero { get; set; } = string.Empty; // F, M, X

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [Column("dni")]
    public int Dni { get; set; }

    [StringLength(100)]
    [Column("localidad")]
    public string? Localidad { get; set; }

    [StringLength(100)]
    [Column("agrupacion")]
    public string? Agrupacion { get; set; }

    [Required(ErrorMessage = "El telefono de emergencia es obligatorio")]
    [StringLength(50)]
    [Column("telefonoEmergencia")]
    public string TelefonoEmergencia { get; set; } = string.Empty;

    // Navegación
    [ForeignKey("IdUsuario")]
    public Usuario Usuario { get; set; } = null!;


  }



}