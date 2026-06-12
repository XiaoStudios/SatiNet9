namespace Sati_Models.Dtos;

/// <summary>
/// Parámetros configurables del algoritmo operativo
/// </summary>
public class ParametersAlgorithmDto
{
    // Período base (tomado del WAM)
    public int Period { get; set; } = 50;
    
    // Parámetros del algoritmo
    public double Pt { get; set; } = 0.05;  // Parámetro de tendencia (porcentaje: 0.05 = 0.05%)
    public double Pr { get; set; } = 0.75;  // Parámetro de rompimiento
    public double Sigma { get; set; } = 2.0; // Multiplicador de desviación estándar
    public double Mp { get; set; } = 1000;   // Importe inicial
    public double Fd { get; set; } = 0.10;   // Factor decreciente (10% pérdida por operación)
    
    // Validaciones
    public bool IsValid()
    {
        return Period > 0 && Pt > 0 && Pr >= 0 && Pr <= 1 
               && Sigma > 0 && Mp > 0 && Fd >= 0 && Fd < 1;
    }
}