using System;
using System.Collections.Generic;

namespace Sati_Models.DBModels;

public partial class UserSymbol
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string SymbolStr { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime AssignedAt { get; set; }

    public virtual AdminUser User { get; set; } = null!;
}
