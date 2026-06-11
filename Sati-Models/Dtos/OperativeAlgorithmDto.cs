namespace Sati_Models.Dtos;

/// <summary>
/// DTO con todos los valores calculados del algoritmo operativo para un registro
/// </summary>
public class OperativeAlgorithmDto
{
    // Identificación
    public int RowNumber { get; set; }
    public string Time { get; set; }
    
    // Precios OHLC (del Rate existente)
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    
    // WAM existente (mantener compatibilidad)
    public double CalculatedWAM { get; set; }
    public double PercentageDifference { get; set; }
    
    // === NUEVOS CAMPOS DEL ALGORITMO ===
    
    // PMPn y extremos
    public double PMPn { get; set; }
    public double MaxP { get; set; }
    public double MinP { get; set; }
    public double? PreviousPMPn { get; set; }
    
    // Rangos Primarios
    public double RPPlus { get; set; }  // RP+
    public double RPMinus { get; set; } // RP-
    
    // Tendencia
    public string Tendencia { get; set; } // "ALZA", "BAJA", "NEUTRO"
    public bool TendenciaAlza { get; set; }
    public bool TendenciaBaja { get; set; }
    
    // Separación
    public double Difn { get; set; }
    public double? PromDifn { get; set; }
    public double? SigmaDifn { get; set; }
    
    // Señal
    public string Signal { get; set; } // "COMPRA", "VENTA", "NINGUNA"
    public bool HasSignal { get; set; }
    public bool IsCompra { get; set; }
    public bool IsVenta { get; set; }
    
    // Importe de acumulación
    public double? ImporteAcumulacion { get; set; }
    public int? NumeroOperacion { get; set; }
}