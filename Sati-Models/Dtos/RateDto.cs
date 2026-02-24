namespace Sati_Models.DTOs;

public class RateDto
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

    public string Time_MT_Api { get; set; } = null!;

    public string Fecha { get; set; } = null!;

    public string Hora { get; set; } = null!;
}