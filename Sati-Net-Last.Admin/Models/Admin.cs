using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sati_Net_Last.Admin.Models;

[Table("admin")] // Apunta a la tabla correcta en MySQL
public class AdminUser
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre_admin")]
    public string NombreAdmin { get; set; } = null!;

    [Column("correo")]
    public string Correo { get; set; } = null!;

    [Column("password")] // MySQL lo lee como 'password', pero C# lo usa como 'PasswordHash'
    public string PasswordHash { get; set; } = null!;

    [Column("rol_id")]
    public int RolId { get; set; }

    public Role? Rol { get; set; }
}
