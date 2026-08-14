using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sati_Net_Last.Admin.Models;

[Table("Admin_Users")]
public class AdminUser
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("Username")]
    public string Username { get; set; } = null!;

    [Column("Email")]
    public string Email { get; set; } = null!;

    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = null!;

    [Column("FullName")]
    public string? FullName { get; set; }

    [Column("IsEnabled")]
    public bool IsEnabled { get; set; }

    [Column("IsAdmin")]
    public bool IsAdmin { get; set; }

    [Column("LastLoginAt")]
    public DateTime? LastLoginAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }
}