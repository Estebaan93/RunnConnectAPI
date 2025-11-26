//Models/TokenRecuperacion.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RunnConnectAPI.Models
{
  [Table("tokens_recuperacion")]
  public class TokenRecuperacion
  {
    [Key]
    [Column("idToken")]
    public int IdToken { get; set; }

    [Required]
    [Column("idUsuario")]
    public int IdUsuario { get; set; }

    [Required]
    [StringLength(255)]
    [Column("token")]
    public string Token { get; set; } = string.Empty;

    [Required]
    [Column("fechaCreacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Required]
    [Column("fechaExpiracion")]
    public DateTime FechaExpiracion { get; set; }

    [Column("usado", TypeName = "tinyint(1)")]
    public bool Usado { get; set; } = false;

    // Navegación
    [ForeignKey("IdUsuario")]
    public virtual Usuario? Usuario { get; set; }
  }
}