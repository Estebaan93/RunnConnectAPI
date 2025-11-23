//Models/PerfilOrganizador.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RunnConnectAPI.Models
{
  /*Perfil especifico a organizadores - Datos legales y comerciales
  Relacion 1:1 con Usuario*/
[Table("perfiles_organizadores")]
  public class PerfilOrganizador
  {
    [Key]
    [Column("idPerfilOrganizador")]
    public int IdPerfilOrganizador { get; set; }

    [Required]
    [Column("idUsuario")]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "La razon social es obligatoria")]
    [StringLength(100, MinimumLength = 2)]
    [Column("razonSocial")]
    public string RazonSocial { get; set; } = string.Empty;

    [StringLength(100)]
    [Column("nombreComercial")]
    public string NombreComercial { get; set; }= string.Empty;

    [Required(ErrorMessage = "El CUIT es obligatorio")]
    [StringLength(30)]
    [Column("cuit_taxid")]
    public string CuitTaxId { get; set; } = string.Empty;

    [StringLength(255)]
    [Column("direccionLegal")]
    public string DireccionLegal { get; set; }= string.Empty;

    // Navegacion
    [ForeignKey("IdUsuario")]
    public Usuario Usuario { get; set; } = null!;
  }  



}