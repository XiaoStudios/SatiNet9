using System;
using System.Collections.Generic;

namespace Sati_Models.DBModels;

public partial class Rate
{
    public int Id { get; set; }

    public string SymbolStr { get; set; } = null!;

    public string Time { get; set; } = null!;

    public double Open { get; set; }

    public double High { get; set; }

    public double Low { get; set; }

    public double Close { get; set; }

    public int TickVolume { get; set; }

    public int Spread { get; set; }

    public int RealVolume { get; set; }

    public string TimeMtApi { get; set; } = null!;

    public string? Fecha { get; set; }

    public string? Hora { get; set; }

    public string TimeFrame { get; set; } = null!;
}
