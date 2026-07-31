using System;
using System.Collections.Generic;

namespace Sati_Models.DBModels;

public partial class MtapiSetting
{
    public int Id { get; set; }

    public string Host { get; set; } = null!;

    public int Port { get; set; }

    public string MtUser { get; set; } = null!;

    public string MtPassword { get; set; } = null!;

    public bool? IsActive { get; set; }

    public string? LastConnectionStatus { get; set; }

    public DateTime? LastConnectionAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<MtapiConnectionLog> MtapiConnectionLogs { get; set; } = new List<MtapiConnectionLog>();
}
