using System;
using System.Collections.Generic;

namespace Sati_Models.DBModels;

public partial class MtapiConnectionLog
{
    public int Id { get; set; }

    public int? SettingsId { get; set; }

    public string EventType { get; set; } = null!;

    public string? Message { get; set; }

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual MtapiSetting? Settings { get; set; }
}
