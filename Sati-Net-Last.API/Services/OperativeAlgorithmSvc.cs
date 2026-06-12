namespace Sati_Net_Last.API.Services;
using Sati_Models.Dtos;
using MTsocketAPI.MT5;

/// <summary>
/// Servicio responsable del cálculo del Algoritmo Operativo
/// Separado del cálculo WAM para mantener responsabilidades claras
/// Soporta tanto cálculo batch (histórico) como incremental (tiempo real)
/// </summary>
public class OperativeAlgorithmSvc
{
private readonly ILogger<OperativeAlgorithmSvc> _logger;
    
    // Cache de estados por símbolo "SYMBOL_PERIOD" (ej: "EURUSD_50")
    private readonly Dictionary<string, StateAlgorithmSvc> _symbolStates;
    private readonly object _lockObject = new object();
    
    public OperativeAlgorithmSvc(ILogger<OperativeAlgorithmSvc> logger)
    {
        _logger = logger;
        _symbolStates = new Dictionary<string, StateAlgorithmSvc>();
    }
    
    #region Métodos Públicos
    
    /// <summary>
    /// Calcula el algoritmo para un conjunto completo de datos históricos (BATCH)
    /// Usado para pantalla de histórico
    /// </summary>
    public List<Rates> CalculateBatch(List<Rates> rates, int period, ParametersAlgorithmDto parameters)
    {
        if (rates == null || rates.Count == 0)
            return rates;
            
        _logger.LogInformation($"Calculando algoritmo BATCH: {rates.Count} registros, período {period}");
        _logger.LogInformation($"Parámetros: pt={parameters.Pt}, pr={parameters.Pr}, sigma={parameters.Sigma}");
        
        // Variables de estado temporal (no se guarda en cache)
        double maxP = 0, minP = 0;
        double? previousPMPn = null;
        List<double> difnHistory = new List<double>();
        int operationCounter = 0;
        
        for (int i = 0; i < rates.Count; i++)
        {
            var rate = rates[i];
            
            // 1. Calcular PMPn usando ventana móvil
            double pmpn = CalculatePMPn(rates, i, period);
            
            // Log detallado de primeras iteraciones para debug
            if (i < 5)
            {
                _logger.LogInformation($"Iteración {i}: CLOSE={rate.CLOSE}, PMPn={pmpn}");
            }
            
            // 2. Inicializar maxP y minP en la primera iteración
            if (i == 0)
            {
                maxP = pmpn;
                minP = pmpn;
            }
            
            // 3. Actualizar maxP y minP
            if (pmpn >= maxP) maxP = pmpn;
            if (pmpn <= minP) minP = pmpn;
            
            // 4. Calcular Rangos Primarios (como porcentaje del precio)
            double rpPlus = pmpn > 0 ? (Math.Abs(pmpn - minP) / pmpn) * 100 : 0;
            double rpMinus = pmpn > 0 ? (Math.Abs(maxP - pmpn) / pmpn) * 100 : 0;
            
            // Log detallado
            if (i < 5)
            {
                _logger.LogInformation($"  maxP={maxP}, minP={minP}, rpPlus={rpPlus:F4}%, rpMinus={rpMinus:F4}%");
            }
            
            // 5. Detectar tendencia y rompimientos
            string tendencia = DetectarTendencia(rpPlus, rpMinus, parameters.Pt, parameters.Pr, 
                                                  ref maxP, ref minP, pmpn, ref operationCounter);
            
            // 6. Calcular separación (Difn)
            double difn = Math.Abs(pmpn - rate.CLOSE);
            difnHistory.Add(difn);
            
            // 7. Calcular estadísticas de Difn
            double? promDifn = null;
            double? sigmaDifn = null;
            
            if (i > 0)
            {
                promDifn = difnHistory.Take(i).Average();
                sigmaDifn = CalculateStandardDeviation(difnHistory.Take(i).ToList());
            }
            
            // 8. Evaluar condiciones de señal
            var signal = EvaluarSenal(tendencia, pmpn, maxP, minP, previousPMPn, 
                                      difn, promDifn, sigmaDifn, parameters.Sigma);
            
            // 9. Calcular importe si hay señal
            double? importeAcumulacion = null;
            if (signal != "NINGUNA")
            {
                importeAcumulacion = CalcularImporte(operationCounter, parameters.Mp, parameters.Fd);
                operationCounter++;
            }
            
            // 10. Asignar valores al rate
            rate.PMPn = pmpn;
            rate.MaxP = maxP;
            rate.MinP = minP;
            rate.PreviousPMPn = previousPMPn;
            rate.RPPlus = rpPlus;
            rate.RPMinus = rpMinus;
            rate.Tendencia = tendencia;
            rate.Difn = difn;
            rate.PromDifn = promDifn;
            rate.SigmaDifn = sigmaDifn;
            rate.Signal = signal;
            rate.ImporteAcumulacion = importeAcumulacion;
            
            // Actualizar para siguiente iteración
            previousPMPn = pmpn;
        }
        
        _logger.LogInformation($"Algoritmo BATCH completado. Señales generadas: " +
            $"{rates.Count(r => r.Signal != "NINGUNA")}");
        
        return rates;
    }
    
    /// <summary>
    /// Calcula el algoritmo incrementalmente para una nueva vela (TIEMPO REAL)
    /// </summary>
    public Rates CalculateIncremental(string symbol, Rates newRate, int period, 
                                      ParametersAlgorithmDto parameters)
    {
        string stateKey = $"{symbol}_{period}";
        
        lock (_lockObject)
        {
            // Obtener o crear estado del símbolo
            if (!_symbolStates.ContainsKey(stateKey))
            {
                _symbolStates[stateKey] = new StateAlgorithmSvc(symbol, period, parameters);
                _logger.LogInformation($"Estado creado para {stateKey}");
            }
            
            var state = _symbolStates[stateKey];
            state.LastUpdate = DateTime.Now;
            
            // Agregar nuevo precio a la cola
            state.RecentCloses.Enqueue(newRate.CLOSE);
            if (state.RecentCloses.Count > period)
            {
                state.RecentCloses.Dequeue(); // Mantener solo últimos n
            }
            
            // Calcular PMPn con los valores en memoria
            double pmpn = CalculatePMPnFromQueue(state.RecentCloses.ToList(), period);
            
            // Primera vez: inicializar extremos
            if (state.PreviousPMPn == null)
            {
                state.MaxP = pmpn;
                state.MinP = pmpn;
            }
            
            // Actualizar extremos
            if (pmpn >= state.MaxP) state.MaxP = pmpn;
            if (pmpn <= state.MinP) state.MinP = pmpn;
            
            // Calcular rangos primarios
            double rpPlus = Math.Abs(pmpn - state.MinP);
            double rpMinus = Math.Abs(state.MaxP - pmpn);
            
            // Detectar tendencia
            double maxP = state.MaxP;
            double minP = state.MinP;
            int operationCounter = state.OperationCounter;
            string tendencia = DetectarTendencia(rpPlus, rpMinus, parameters.Pt, parameters.Pr,
                                                  ref maxP, ref minP, pmpn, 
                                                  ref operationCounter);
            state.MaxP = maxP;
            state.MinP = minP;
            state.OperationCounter = operationCounter;
            
            // Calcular Difn
            double difn = Math.Abs(pmpn - newRate.CLOSE);
            state.DifnHistory.Add(difn);
            
            // Estadísticas
            double? promDifn = null;
            double? sigmaDifn = null;
            
            if (state.DifnHistory.Count > 1)
            {
                promDifn = state.DifnHistory.Average();
                sigmaDifn = CalculateStandardDeviation(state.DifnHistory);
            }
            
            // Evaluar señal
            var signal = EvaluarSenal(tendencia, pmpn, state.MaxP, state.MinP, 
                                      state.PreviousPMPn, difn, promDifn, sigmaDifn, 
                                      parameters.Sigma);
            
            // Calcular importe
            double? importeAcumulacion = null;
            if (signal != "NINGUNA")
            {
                importeAcumulacion = CalcularImporte(state.OperationCounter, parameters.Mp, parameters.Fd);
                state.OperationCounter++;
            }
            
            // Asignar valores
            newRate.PMPn = pmpn;
            newRate.MaxP = state.MaxP;
            newRate.MinP = state.MinP;
            newRate.PreviousPMPn = state.PreviousPMPn;
            newRate.RPPlus = rpPlus;
            newRate.RPMinus = rpMinus;
            newRate.Tendencia = tendencia;
            newRate.Difn = difn;
            newRate.PromDifn = promDifn;
            newRate.SigmaDifn = sigmaDifn;
            newRate.Signal = signal;
            newRate.ImporteAcumulacion = importeAcumulacion;
            
            // Actualizar estado
            state.CurrentPMPn = pmpn;
            state.PreviousPMPn = pmpn;
            state.CurrentTrend = tendencia;
            
            return newRate;
        }
    }
    
    /// <summary>
    /// Inicializar estado desde histórico (útil al inicio del día)
    /// </summary>
    public void InitializeFromHistory(string symbol, List<Rates> historicalRates, 
                                      int period, ParametersAlgorithmDto parameters)
    {
        if (historicalRates == null || historicalRates.Count == 0)
            return;
            
        string stateKey = $"{symbol}_{period}";
        
        lock (_lockObject)
        {
            var state = new StateAlgorithmSvc(symbol, period, parameters);
            
            // Tomar últimos 'period' precios
            var recentPrices = historicalRates
                .TakeLast(period)
                .Select(r => r.CLOSE)
                .ToList();
            
            foreach (var price in recentPrices)
            {
                state.RecentCloses.Enqueue(price);
            }
            
            // Inicializar con el último registro del histórico
            var lastRate = historicalRates.Last();
            state.CurrentPMPn = lastRate.PMPn;
            state.PreviousPMPn = lastRate.PreviousPMPn;
            state.MaxP = lastRate.MaxP;
            state.MinP = lastRate.MinP;
            state.CurrentTrend = lastRate.Tendencia;
            
            // Copiar historial de Difn
            state.DifnHistory = historicalRates
                .Where(r => r.Difn > 0)
                .Select(r => r.Difn)
                .ToList();
            
            _symbolStates[stateKey] = state;
            
            _logger.LogInformation($"Estado inicializado desde histórico para {stateKey}: " +
                $"{state.RecentCloses.Count} precios, {state.DifnHistory.Count} Difn");
        }
    }
    
    /// <summary>
    /// Limpiar estado de un símbolo
    /// </summary>
    public void ClearState(string symbol, int period)
    {
        string stateKey = $"{symbol}_{period}";
        lock (_lockObject)
        {
            if (_symbolStates.ContainsKey(stateKey))
            {
                _symbolStates.Remove(stateKey);
                _logger.LogInformation($"Estado limpiado para {stateKey}");
            }
        }
    }
    
    #endregion
    
    #region Métodos Privados de Cálculo
    
    /// <summary>
    /// Calcula PMPn usando ventana móvil en el array de rates
    /// </summary>
    private double CalculatePMPn(List<Rates> rates, int currentIndex, int period)
    {
        int actualPeriod = period + 1; // 21 para período 20, 51 para período 50
        int valuesAvailable = currentIndex + 1;
        int valuesForPMPn = Math.Min(valuesAvailable, actualPeriod);
        
        double sumProduct = 0;
        int sumOfMultipliers = 0;
        
        for (int j = 0; j < valuesForPMPn; j++)
        {
            int multiplier = j + 1;
            int priceIndex = currentIndex - (valuesForPMPn - 1) + j;
            double closePrice = rates[priceIndex].CLOSE;
            
            sumProduct += multiplier * closePrice;
            sumOfMultipliers += multiplier;
        }
        
        return sumProduct / sumOfMultipliers;
    }
    
    /// <summary>
    /// Calcula PMPn desde una cola en memoria (para tiempo real)
    /// </summary>
    private double CalculatePMPnFromQueue(List<double> recentCloses, int period)
    {
        if (recentCloses.Count == 0)
            return 0;
            
        int actualPeriod = period + 1;
        int valuesForPMPn = Math.Min(recentCloses.Count, actualPeriod);
        
        double sumProduct = 0;
        int sumOfMultipliers = 0;
        
        // Usar los últimos valuesForPMPn valores
        var valuesToUse = recentCloses.TakeLast(valuesForPMPn).ToList();
        
        for (int j = 0; j < valuesToUse.Count; j++)
        {
            int multiplier = j + 1;
            sumProduct += multiplier * valuesToUse[j];
            sumOfMultipliers += multiplier;
        }
        
        return sumProduct / sumOfMultipliers;
    }
    
    /// <summary>
    /// Detecta tendencia y gestiona rompimientos
    /// </summary>
    private string DetectarTendencia(double rpPlus, double rpMinus, double pt, double pr,
                                     ref double maxP, ref double minP, double pmpn,
                                     ref int operationCounter)
    {
        string tendencia = "NEUTRO";
        
        // Verificar tendencia al alza
        if (rpPlus >= pt)
        {
            tendencia = "ALZA";
        }
        // Verificar tendencia a la baja
        else if (rpMinus >= pt)
        {
            tendencia = "BAJA";
        }
        
        // Detectar rompimiento de tendencia
        if (rpMinus > 0 && rpPlus / rpMinus >= pr)
        {
            // Rompimiento a la baja
            maxP = pmpn;
            operationCounter = 0; // Resetear contador
        }
        else if (rpPlus > 0 && rpMinus / rpPlus >= pr)
        {
            // Rompimiento al alza
            minP = pmpn;
            operationCounter = 0; // Resetear contador
        }
        
        return tendencia;
    }
    
    /// <summary>
    /// Evalúa las 4 condiciones para generar señal
    /// </summary>
    private string EvaluarSenal(string tendencia, double pmpn, double maxP, double minP,
                                double? previousPMPn, double difn, double? promDifn,
                                double? sigmaDifn, double sigmaMultiplier)
    {
        // Condiciones para COMPRA
        if (tendencia == "ALZA")
        {
            // Condición 1: Tendencia al alza ✓ (ya verificado)
            
            // Condición 2: PMPn = max(P)
            bool condition2 = Math.Abs(pmpn - maxP) < 0.00001;
            
            // Condición 3: PMPn ≠ PMPn-1
            bool condition3 = previousPMPn == null || Math.Abs(pmpn - previousPMPn.Value) > 0.00001;
            
            // Condición 4: Difn dentro del rango esperado
            bool condition4 = true; // Por defecto true si no hay historial
            if (promDifn.HasValue && sigmaDifn.HasValue)
            {
                double upperBound = promDifn.Value + (sigmaMultiplier * sigmaDifn.Value);
                double lowerBound = promDifn.Value - (sigmaMultiplier * sigmaDifn.Value);
                condition4 = difn >= lowerBound && difn <= upperBound;
            }
            
            if (condition2 && condition3 && condition4)
            {
                return "COMPRA";
            }
        }
        
        // Condiciones para VENTA
        if (tendencia == "BAJA")
        {
            // Condición 1: Tendencia a la baja ✓ (ya verificado)
            
            // Condición 2: PMPn = min(P)
            bool condition2 = Math.Abs(pmpn - minP) < 0.00001;
            
            // Condición 3: PMPn ≠ PMPn-1
            bool condition3 = previousPMPn == null || Math.Abs(pmpn - previousPMPn.Value) > 0.00001;
            
            // Condición 4: Difn dentro del rango esperado
            bool condition4 = true;
            if (promDifn.HasValue && sigmaDifn.HasValue)
            {
                double upperBound = promDifn.Value + (sigmaMultiplier * sigmaDifn.Value);
                double lowerBound = promDifn.Value - (sigmaMultiplier * sigmaDifn.Value);
                condition4 = difn >= lowerBound && difn <= upperBound;
            }
            
            if (condition2 && condition3 && condition4)
            {
                return "VENTA";
            }
        }
        
        return "NINGUNA";
    }
    
    /// <summary>
    /// Calcula el importe de acumulación según la fórmula
    /// </summary>
    private double CalcularImporte(int operationNumber, double mp, double fd)
    {
        // IMP_i = mp × (1 - fd)^(i-1)
        double importe = mp * Math.Pow(1 - fd, operationNumber);
        
        // Importe mínimo de 1 euro
        return Math.Max(importe, 1.0);
    }
    
    /// <summary>
    /// Calcula desviación estándar
    /// </summary>
    private double CalculateStandardDeviation(List<double> values)
    {
        if (values.Count < 2)
            return 0;
            
        double avg = values.Average();
        double sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }
    
    #endregion
}