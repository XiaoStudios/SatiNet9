using System;
using System.Collections.Generic;

namespace Sati_Models.DBModels;

public partial class AdminUser
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Symbol { get; set; }

    public bool? IsEnabled { get; set; }

    public bool IsAdmin { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
