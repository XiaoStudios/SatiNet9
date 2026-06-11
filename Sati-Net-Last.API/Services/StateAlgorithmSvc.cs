namespace Sati_Net_Last.API.Services;
using Sati_Models.Dtos;

/// <summary>
/// Estado en memoria del algoritmo para un símbolo específico
/// Permite cálculo incremental en tiempo real
/// </summary>
public class StateAlgorithmSvc
{
    public string Symbol { get; set; }
    public int Period { get; set; }
    public DateTime LastUpdate { get; set; }
    
    // Parámetros del algoritmo
    public ParametersAlgorithmDto Parameters { get; set; }
    
    // Cola de precios recientes (ventana móvil)
    public Queue<double> RecentCloses { get; set; }
    
    // Estado de extremos
    public double MaxP { get; set; }
    public double MinP { get; set; }
    public double CurrentPMPn { get; set; }
    public double? PreviousPMPn { get; set; }
    
    // Historial de separaciones para estadísticas
    public List<double> DifnHistory { get; set; }
    
    // Contador de operaciones en tendencia actual
    public int OperationCounter { get; set; }
    public string CurrentTrend { get; set; } // "ALZA", "BAJA", "NEUTRO"
    
    public StateAlgorithmSvc(string symbol, int period, ParametersAlgorithmDto parameters)
    {
        Symbol = symbol;
        Period = period;
        Parameters = parameters ?? new ParametersAlgorithmDto();
        RecentCloses = new Queue<double>();
        DifnHistory = new List<double>();
        OperationCounter = 0;
        CurrentTrend = "NEUTRO";
        LastUpdate = DateTime.Now;
    }
    
    /// <summary>
    /// Resetear estado (útil cuando cambia el día o los parámetros)
    /// </summary>
    public void Reset()
    {
        RecentCloses.Clear();
        DifnHistory.Clear();
        OperationCounter = 0;
        CurrentTrend = "NEUTRO";
        PreviousPMPn = null;
    }
}