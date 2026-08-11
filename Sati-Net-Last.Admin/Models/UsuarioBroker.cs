using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sati_Net_Last.Admin.Models;

[Table("usuarios_broker")]
public class UsuarioBroker
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre_completo")]
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [StringLength(200)]
    public string NombreCompleto { get; set; } = null!;

    [Column("correo")]
    [Required(ErrorMessage = "El correo es requerido")]
    [EmailAddress(ErrorMessage = "El correo debe ser válido")]
    [StringLength(200)]
    public string Correo { get; set; } = null!;

    [Column("par_moneda")]
    [Required(ErrorMessage = "El par de moneda es requerido")]
    [StringLength(50)]
    public string ParMoneda { get; set; } = null!;

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("fecha_registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
