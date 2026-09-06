using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sati_Net_Last.Admin.Models;

[Table("Mtapi_Settings")]
public class MtapiSetting
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("Host")]
    [Required(ErrorMessage = "El host es obligatorio.")]
    [StringLength(255, ErrorMessage = "El host no puede exceder 255 caracteres.")]
    public string Host { get; set; } = null!;

    [Column("Port")]
    [Required(ErrorMessage = "El puerto es obligatorio.")]
    [Range(1, 65535, ErrorMessage = "El puerto debe estar entre 1 y 65535.")]
    public int Port { get; set; }

    [Column("MtUser")]
    [Required(ErrorMessage = "El usuario de MetaTrader es obligatorio.")]
    [StringLength(255, ErrorMessage = "El usuario no puede exceder 255 caracteres.")]
    public string MtUser { get; set; } = null!;

    [Column("MtPassword")]
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(255, ErrorMessage = "La contraseña no puede exceder 255 caracteres.")]
    [DataType(DataType.Password)]
    public string MtPassword { get; set; } = null!;

    [Column("IsActive")]
    public bool IsActive { get; set; }

    [Column("LastConnectionStatus")]
    [StringLength(255)]
    public string? LastConnectionStatus { get; set; }

    [Column("LastConnectionAt")]
    public DateTime? LastConnectionAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }
}
