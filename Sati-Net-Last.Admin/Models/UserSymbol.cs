using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sati_Net_Last.Admin.Models;

[Table("User_Symbols")]
public class UserSymbol
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }

    [Column("SymbolStr")]
    public string SymbolStr { get; set; } = null!;

    [Column("IsActive")]
    public bool IsActive { get; set; }

    [Column("AssignedAt")]
    public DateTime AssignedAt { get; set; }

    public AdminUser? User { get; set; }
}