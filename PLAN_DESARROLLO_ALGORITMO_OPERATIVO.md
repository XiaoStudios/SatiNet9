# 📋 Plan de Desarrollo - Algoritmo Operativo en Tiempo Real

**Proyecto:** Sati-Net-Last - Integración de Algoritmo Operativo  
**Fecha:** 26 de Mayo, 2026  
**Estrategia:** Opción 3 - Híbrida (ÓPTIMA) 🎯  

---

## 🎯 Objetivos del Proyecto

1. ✅ Agregar parámetros del algoritmo operativo a los filtros
2. ✅ Calcular y mostrar columnas expandidas con datos del algoritmo
3. ✅ Implementar checkboxes para control visual (velas y señales)
4. ✅ Separar responsabilidades: WAM vs PMPn
5. ✅ Preparar arquitectura para cálculo en tiempo real
6. ⏸️ Gráfico mejorado (dejar para fase final)

---

## 📐 Arquitectura Propuesta

### **🏗️ Arquitectura Centralizada (Backend Único + N Frontends)**

**Principio clave:** SATI API procesa datos de MetaTrader **UNA SOLA VEZ** y transmite a todos los clientes conectados.

```
┌────────────────────────┐
│   MetaTrader API       │  🔌 Entrada única de datos de mercado
│   (MT5 Terminal)       │
└──────────┬─────────────┘
           │ Socket/API Connection
           │
           ▼
┌─────────────────────────────────────────────────────────────┐
│                 🎯 SATI API (Backend Centralizado)          │
│              Sati-Net-Last.API - ASP.NET Core               │
│─────────────────────────────────────────────────────────────│
│  1️⃣  Recibe vela de MT API                                  │
│  2️⃣  Procesa algoritmo UNA SOLA VEZ:                        │
│      ├─ PMPn, max/min, RP+, RP-                            │
│      ├─ Detección de tendencia                             │
│      ├─ Evaluación de señales                              │
│      └─ Cálculo de importes                                │
│  3️⃣  Guarda en DB (persistencia + auditoría)                │
│  4️⃣  Broadcast via SignalR a TODOS los frontends           │
│                                                              │
│  Componentes:                                                │
│  ├─ MTSocketAPI5 (conexión a MT5)                          │
│  ├─ OperativeAlgorithmSvc (lógica de cálculo)          │
│  ├─ TerminalRepo (persistencia DB)                          │
│  ├─ MetaTraderHub (SignalR Hub)                            │
│  └─ MetaTraderController (API REST)                         │
└────────┬────────────────────────┬───────────────────────────┘
         │                        │
         │ SignalR                │ HTTP REST
         │ (tiempo real)          │ (histórico/consultas)
         │                        │
    ┌────┴────┐              ┌────┴────┐
    │         │              │         │
    ▼         ▼              ▼         ▼
┌─────────┐ ┌─────────┐  ┌─────────┐ ┌──────────┐
│Frontend │ │Frontend │  │Frontend │ │ Futuro   │
│   Web   │ │   Web   │  │ Mobile  │ │ Desktop  │
│(History)│ │(Trader) │  │   App   │ │   App    │
│─────────│ │─────────│  │─────────│ │──────────│
│Consulta │ │SignalR  │  │SignalR  │ │ SignalR  │
│histórico│ │Tiempo   │  │Tiempo   │ │ Tiempo   │
│via REST │ │Real     │  │Real     │ │ Real     │
│Gráficos │ │Señales  │  │Alertas  │ │ Trading  │
│Filtros  │ │Monitor  │  │Push     │ │ Avanzado │
└─────────┘ └─────────┘  └─────────┘ └──────────┘
     ↑           ↑            ↑           ↑
     └───────────┴────────────┴───────────┘
          Todos consumen los mismos datos
          procesados por SATI API
```

### **✅ Ventajas de Esta Arquitectura**

| Aspecto | Beneficio |
|---------|----------|
| **Procesamiento** | 1 cálculo → N frontends (eficiencia máxima) |
| **Escalabilidad** | Agregar clientes sin aumentar carga de procesamiento |
| **Consistencia** | Todos los clientes ven exactamente los mismos datos |
| **Mantenibilidad** | Lógica de negocio centralizada en el backend |
| **Independencia** | Frontends pueden conectar/desconectar sin afectar el backend |
| **Auditoría** | Todo guardado en DB, sin importar cuántos clientes hay |
| **Confiabilidad** | Backend corre 24/7, si frontend cae, backend sigue procesando |

### **📋 Separación de Responsabilidades**

#### **SATI API (Backend)** - `Sati-Net-Last.API`

```
Responsabilidades:
├─ Conectar a MetaTrader API
├─ Calcular algoritmo operativo (una sola vez)
├─ Persistir datos en DB
├─ Transmitir actualizaciones via SignalR
├─ Exponer API REST para consultas históricas
└─ Gestionar estado en memoria (cache)

Componentes:
├─ Services/
│  ├─ OperativeAlgorithmSvc.cs (cálculo batch + incremental)
│  ├─ AlgoritmoState.cs (cache en memoria)
│  └─ RealTimeAlgoritmoHostedService.cs (procesamiento continuo)
├─ MTRepositories/
│  ├─ MTRepo.cs (conexión MT API - WAM existente)
│  └─ TerminalRepo.cs (persistencia DB)
├─ Controllers/
│  └─ MetaTraderController.cs (API REST)
└─ Hubs/
   └─ MetaTraderHub.cs (SignalR Hub)
```

#### **Frontends (Múltiples)** - `Sati-Net-Last.Web` y futuros

```
Responsabilidades:
├─ Visualización de datos
├─ Interacción con el usuario (filtros, gráficos)
├─ Consumo de API REST (histórico)
├─ Suscripción a SignalR (tiempo real)
└─ Alertas y notificaciones

NO son responsables de:
✗ Calcular algoritmos
✗ Conectar a MetaTrader
✗ Persistir datos
✗ Validación de señales
```

---

## 🏗️ FASE 1: Preparación de Modelos y DTOs

**Duración estimada:** 4-6 horas  
**Prioridad:** ALTA  

### 1.1 Crear DTOs para Algoritmo Operativo

**Archivo:** `/Sati-Models/Dtos/AlgoritmoOperativoDto.cs` (NUEVO)

```csharp
namespace Sati_Models.Dtos;

/// <summary>
/// DTO con todos los valores calculados del algoritmo operativo para un registro
/// </summary>
public class AlgoritmoOperativoDto
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
```

### 1.2 Crear DTO para Parámetros del Algoritmo

**Archivo:** `/Sati-Models/Dtos/AlgoritmoParametersDto.cs` (NUEVO)

```csharp
namespace Sati_Models.Dtos;

/// <summary>
/// Parámetros configurables del algoritmo operativo
/// </summary>
public class AlgoritmoParametersDto
{
    // Período base (tomado del WAM)
    public int Period { get; set; } = 50;
    
    // Parámetros del algoritmo
    public double Pt { get; set; } = 0.30;  // Parámetro de tendencia
    public double Pr { get; set; } = 0.75;  // Parámetro de rompimiento
    public double Sigma { get; set; } = 2.0; // Multiplicador de desviación estándar
    public double Mp { get; set; } = 1000;   // Importe inicial
    public double Fd { get; set; } = 0.95;   // Factor decreciente
    
    // Validaciones
    public bool IsValid()
    {
        return Period > 0 && Pt > 0 && Pr >= 0 && Pr <= 1 
               && Sigma > 0 && Mp > 0 && Fd >= 0 && Fd < 1;
    }
}
```

### 1.3 Extender Rates para incluir datos del algoritmo

**Archivo:** `/MT5socketAPI/Rates.cs` (MODIFICAR)

```csharp
// Agregar propiedades al final de la clase Rates existente:

// Algoritmo Operativo (nuevo)
public double PMPn { get; set; }
public double MaxP { get; set; }
public double MinP { get; set; }
public double? PreviousPMPn { get; set; }
public double RPPlus { get; set; }
public double RPMinus { get; set; }
public string Tendencia { get; set; }
public double Difn { get; set; }
public double? PromDifn { get; set; }
public double? SigmaDifn { get; set; }
public string Signal { get; set; }
public double? ImporteAcumulacion { get; set; }
```

---

## 🏗️ FASE 2: Servicio de Algoritmo Operativo

**Duración estimada:** 8-12 horas  
**Prioridad:** ALTA  

### 2.1 Crear Clase de Estado del Algoritmo

**Archivo:** `/Sati-Net-Last.API/Services/AlgoritmoState.cs` (NUEVO)

```csharp
namespace Sati_Net_Last.API.Services;

/// <summary>
/// Estado en memoria del algoritmo para un símbolo específico
/// Permite cálculo incremental en tiempo real
/// </summary>
public class AlgoritmoState
{
    public string Symbol { get; set; }
    public int Period { get; set; }
    public DateTime LastUpdate { get; set; }
    
    // Parámetros del algoritmo
    public AlgoritmoParametersDto Parameters { get; set; }
    
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
    
    public AlgoritmoState(string symbol, int period, AlgoritmoParametersDto parameters)
    {
        Symbol = symbol;
        Period = period;
        Parameters = parameters ?? new AlgoritmoParametersDto();
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
```

### 2.2 Crear Servicio de Algoritmo Operativo

**Archivo:** `/Sati-Net-Last.API/Services/OperativeAlgorithmSvc.cs` (NUEVO)

```csharp
namespace Sati_Net_Last.API.Services;

/// <summary>
/// Servicio responsable del cálculo del Algoritmo Operativo
/// Separado del cálculo WAM para mantener responsabilidades claras
/// Soporta tanto cálculo batch (histórico) como incremental (tiempo real)
/// </summary>
public class OperativeAlgorithmSvc
{
    private readonly ILogger<OperativeAlgorithmSvc> _logger;
    
    // Cache de estados por símbolo "SYMBOL_PERIOD" (ej: "EURUSD_50")
    private readonly Dictionary<string, AlgoritmoState> _symbolStates;
    private readonly object _lockObject = new object();
    
    public OperativeAlgorithmSvc(ILogger<OperativeAlgorithmSvc> logger)
    {
        _logger = logger;
        _symbolStates = new Dictionary<string, AlgoritmoState>();
    }
    
    #region Métodos Públicos
    
    /// <summary>
    /// Calcula el algoritmo para un conjunto completo de datos históricos (BATCH)
    /// Usado para pantalla de histórico
    /// </summary>
    public List<Rates> CalculateBatch(List<Rates> rates, int period, AlgoritmoParametersDto parameters)
    {
        if (rates == null || rates.Count == 0)
            return rates;
            
        _logger.LogInformation($"Calculando algoritmo BATCH: {rates.Count} registros, período {period}");
        
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
            
            // 2. Inicializar maxP y minP en la primera iteración
            if (i == 0)
            {
                maxP = pmpn;
                minP = pmpn;
            }
            
            // 3. Actualizar maxP y minP
            if (pmpn >= maxP) maxP = pmpn;
            if (pmpn <= minP) minP = pmpn;
            
            // 4. Calcular Rangos Primarios
            double rpPlus = Math.Abs(pmpn - minP);
            double rpMinus = Math.Abs(maxP - pmpn);
            
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
                                      AlgoritmoParametersDto parameters)
    {
        string stateKey = $"{symbol}_{period}";
        
        lock (_lockObject)
        {
            // Obtener o crear estado del símbolo
            if (!_symbolStates.ContainsKey(stateKey))
            {
                _symbolStates[stateKey] = new AlgoritmoState(symbol, period, parameters);
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
            string tendencia = DetectarTendencia(rpPlus, rpMinus, parameters.Pt, parameters.Pr,
                                                  ref state.MaxP, ref state.MinP, pmpn, 
                                                  ref state.OperationCounter);
            
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
                                      int period, AlgoritmoParametersDto parameters)
    {
        if (historicalRates == null || historicalRates.Count == 0)
            return;
            
        string stateKey = $"{symbol}_{period}";
        
        lock (_lockObject)
        {
            var state = new AlgoritmoState(symbol, period, parameters);
            
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
```

---

## 🏗️ FASE 3: Modificaciones en el Backend API

**Duración estimada:** 4-6 horas  
**Prioridad:** ALTA  

### 3.1 Registrar Servicio en Program.cs

**Archivo:** `/Sati-Net-Last.API/Program.cs`

```csharp
// Agregar después de AddSignalR():

// Registrar servicio de algoritmo operativo como Singleton (mantiene estado)
builder.Services.AddSingleton<OperativeAlgorithmSvc>();
```

### 3.2 Agregar Endpoint en MetaTraderController

**Archivo:** `/Sati-Net-Last.API/Controllers/MetaTraderController.cs`

```csharp
// Inyectar servicio
private readonly OperativeAlgorithmSvc _algoritmoService;

public MetaTraderController(
    ILogger<MetaTraderController> logger,
    IMTRepo mtRepo,
    OperativeAlgorithmSvc algoritmoService) // NUEVO
{
    _logger = logger;
    _mtRepo = mtRepo;
    _algoritmoService = algoritmoService;
}

// NUEVO ENDPOINT
[HttpGet("GetDatePriceHistoryWithAlgorithm")]
public async Task<IActionResult> GetDatePriceHistoryWithAlgorithm(
    DateTime dateFilter, 
    string symbolStr, 
    int wamPeriod,
    [FromQuery] double? pt = null,
    [FromQuery] double? pr = null,
    [FromQuery] double? sigma = null,
    [FromQuery] double? mp = null,
    [FromQuery] double? fd = null)
{
    try
    {
        _logger.LogInformation($"GetDatePriceHistoryWithAlgorithm: {symbolStr}, {dateFilter:yyyy-MM-dd}, WAM {wamPeriod}");
        
        // 1. Obtener datos básicos con WAM (método existente)
        var rates = await _mtRepo.GetDatePriceHistoryAsync(dateFilter, symbolStr, wamPeriod);
        
        if (rates == null || rates.Count == 0)
        {
            return NotFound("No hay datos disponibles");
        }
        
        // 2. Configurar parámetros del algoritmo
        var parameters = new AlgoritmoParametersDto
        {
            Period = wamPeriod,
            Pt = pt ?? 0.30,
            Pr = pr ?? 0.75,
            Sigma = sigma ?? 2.0,
            Mp = mp ?? 1000,
            Fd = fd ?? 0.95
        };
        
        if (!parameters.IsValid())
        {
            return BadRequest("Parámetros del algoritmo inválidos");
        }
        
        // 3. Calcular algoritmo operativo (BATCH)
        rates = _algoritmoService.CalculateBatch(rates, wamPeriod, parameters);
        
        _logger.LogInformation($"Algoritmo calculado. Señales: {rates.Count(r => r.Signal != "NINGUNA")}");
        
        return Ok(rates);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error en GetDatePriceHistoryWithAlgorithm");
        return StatusCode(500, "Error al calcular el algoritmo");
    }
}
```

### 3.3 Mantener Endpoint Existente Sin Cambios

**Importante:** El endpoint `GetDatePriceHistory` original debe **mantenerse sin cambios** para:
- Compatibilidad con código existente
- No romper funcionalidad actual
- Permitir transición gradual

---

## 🏗️ FASE 4: Modificaciones en el Frontend Web

**Duración estimada:** 6-8 horas  
**Prioridad:** ALTA  

### 4.1 Actualizar Controlador Web

**Archivo:** `/Sati-Net-Last.Web/Controllers/HistoryMTController.cs`

```csharp
// NUEVO MÉTODO para obtener datos con algoritmo
[HttpGet]
public async Task<IActionResult> GetDatePriceHistoryWithAlgorithm(
    DateTime dateFilter, 
    string symbolStr, 
    int wamPeriod,
    double? pt = null,
    double? pr = null,
    double? sigma = null,
    double? mp = null,
    double? fd = null)
{
    try
    {
        var client = _httpClientFactory.CreateClient("BackendAPI");
        
        // Construir query string con parámetros opcionales
        var queryParams = new List<string>
        {
            $"dateFilter={dateFilter:yyyy-MM-dd}",
            $"symbolStr={symbolStr}",
            $"wamPeriod={wamPeriod}"
        };
        
        if (pt.HasValue) queryParams.Add($"pt={pt.Value}");
        if (pr.HasValue) queryParams.Add($"pr={pr.Value}");
        if (sigma.HasValue) queryParams.Add($"sigma={sigma.Value}");
        if (mp.HasValue) queryParams.Add($"mp={mp.Value}");
        if (fd.HasValue) queryParams.Add($"fd={fd.Value}");
        
        var queryString = string.Join("&", queryParams);
        var response = await client.GetAsync($"api/MetaTrader/GetDatePriceHistoryWithAlgorithm?{queryString}");
        
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<object>();
            return Ok(data);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning($"API returned {response.StatusCode}: {errorContent}");
            return StatusCode((int)response.StatusCode, errorContent);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener datos con algoritmo");
        return StatusCode(500, "Error al cargar datos");
    }
}
```

### 4.2 Actualizar Vista - Agregar Filtros de Algoritmo

**Archivo:** `/Sati-Net-Last.Web/Views/HistoryMT/Index.cshtml`

Agregar después de los filtros existentes:

```html
<!-- NUEVA SECCIÓN: Parámetros del Algoritmo Operativo -->
<div class="mt-3 p-3" id="algorithmParamsSection"
    style="background: #fff3cd; border-left: 4px solid #ffc107; border-radius: 6px;">
    
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px;">
        <h5 style="color: #856404; font-weight: 600; margin: 0;">
            ⚙️ Parámetros del Algoritmo Operativo
        </h5>
        
        <!-- Checkbox para habilitar/deshabilitar -->
        <div class="form-check form-switch">
            <input class="form-check-input" type="checkbox" id="enableAlgorithm" checked>
            <label class="form-check-label" for="enableAlgorithm" style="font-weight: 600;">
                Activar Algoritmo
            </label>
        </div>
    </div>
    
    <div id="algorithmFields" style="display: grid; grid-template-columns: repeat(6, 1fr); gap: 15px;">
        <!-- Período PMPn (solo lectura, tomado de WAM) -->
        <div class="form-group">
            <label style="display:block; margin-bottom:5px; font-weight:600; color:#856404; font-size:13px;">
                Período PMPn
            </label>
            <input type="text" id="pmpnPeriod" class="form-control" readonly
                style="background: #e9ecef; font-weight: 600;"
                title="Tomado del Período WAM seleccionado">
        </div>
        
        <!-- pt (Parámetro de Tendencia) -->
        <div class="form-group">
            <label style="display:block; margin-bottom:5px; font-weight:600; color:#856404; font-size:13px;">
                pt (Tendencia)
            </label>
            <input type="number" id="paramPt" class="form-control" value="0.30" step="0.01" min="0"
                style="padding:8px; border: 2px solid #dee2e6; border-radius: 4px; font-size: 13px;"
                title="Umbral para establecer tendencia (RP ≥ pt)">
        </div>
        
        <!-- pr (Parámetro de Rompimiento) -->
        <div class="form-group">
            <label style="display:block; margin-bottom:5px; font-weight:600; color:#856404; font-size:13px;">
                pr (Rompimiento %)
            </label>
            <input type="number" id="paramPr" class="form-control" value="0.75" step="0.01" min="0" max="1"
                style="padding:8px; border: 2px solid #dee2e6; border-radius: 4px; font-size: 13px;"
                title="Parámetro de rompimiento de tendencia">
        </div>
        
        <!-- σ (Sigma) -->
        <div class="form-group">
            <label style="display:block; margin-bottom:5px; font-weight:600; color:#856404; font-size:13px;">
                σ (Sigma)
            </label>
            <input type="number" id="paramSigma" class="form-control" value="2.0" step="0.1" min="0"
                style="padding:8px; border: 2px solid #dee2e6; border-radius: 4px; font-size: 13px;"
                title="Multiplicador de desviación estándar">
        </div>
        
        <!-- mp (Importe Inicial) -->
        <div class="form-group">
            <label style="display:block; margin-bottom:5px; font-weight:600; color:#856404; font-size:13px;">
                mp (Importe €)
            </label>
            <input type="number" id="paramMp" class="form-control" value="1000" step="100" min="1"
                style="padding:8px; border: 2px solid #dee2e6; border-radius: 4px; font-size: 13px;"
                title="Importe inicial para acumulación">
        </div>
        
        <!-- fd (Factor Decreciente) -->
        <div class="form-group">
            <label style="display:block; margin-bottom:5px; font-weight:600; color:#856404; font-size:13px;">
                fd (Factor)
            </label>
            <input type="number" id="paramFd" class="form-control" value="0.95" step="0.01" min="0" max="0.99"
                style="padding:8px; border: 2px solid #dee2e6; border-radius: 4px; font-size: 13px;"
                title="Factor decreciente de importe">
        </div>
    </div>
    
    <div class="mt-2" style="display: flex; gap: 10px;">
        <button class="btn btn-sm btn-warning" onclick="resetAlgorithmParams()">
            ↺ Restaurar valores por defecto
        </button>
        <small style="color: #856404; align-self: center;">
            ℹ️ El Período PMPn usa automáticamente el valor del Período WAM
        </small>
    </div>
</div>

<!-- NUEVA SECCIÓN: Opciones de Visualización -->
<div class="mt-3 p-3" style="background: #e7f3ff; border-left: 4px solid #2196F3; border-radius: 6px;">
    <h5 style="color: #0d47a1; font-weight: 600; margin-bottom: 15px;">
        👁️ Opciones de Visualización
    </h5>
    
    <div style="display: flex; gap: 30px;">
        <div class="form-check">
            <input class="form-check-input" type="checkbox" id="showCandlesChart" checked>
            <label class="form-check-label" for="showCandlesChart" style="font-weight: 500;">
                📈 Mostrar velas en gráfico
            </label>
        </div>
        
        <div class="form-check">
            <input class="form-check-input" type="checkbox" id="showSignalsChart" checked>
            <label class="form-check-label" for="showSignalsChart" style="font-weight: 500;">
                🎯 Mostrar señales de compra/venta
            </label>
        </div>
        
        <div class="form-check">
            <input class="form-check-input" type="checkbox" id="highlightSignalRows" checked>
            <label class="form-check-label" for="highlightSignalRows" style="font-weight: 500;">
                ✨ Resaltar filas con señales en tabla
            </label>
        </div>
    </div>
</div>
```

### 4.3 Actualizar Tabla - Agregar Columnas del Algoritmo

Reemplazar el `<thead>` de la tabla:

```html
<thead style="background: #5a67d8; color: white;">
    <tr>
        <!-- Columnas básicas -->
        <th rowspan="2" style="padding: 10px; text-align: center; width: 50px;">#</th>
        <th rowspan="2" style="padding: 10px; text-align: left; width: 130px;">Time</th>
        <th rowspan="2" style="padding: 10px; text-align: right;">Open</th>
        <th rowspan="2" style="padding: 10px; text-align: right;">High</th>
        <th rowspan="2" style="padding: 10px; text-align: right;">Low</th>
        <th rowspan="2" style="padding: 10px; text-align: right;">Close</th>
        
        <!-- Columnas WAM existentes -->
        <th colspan="2" style="padding: 10px; text-align: center; background: #4a55c4;">Compuesto</th>
        
        <!-- NUEVAS COLUMNAS DEL ALGORITMO -->
        <th colspan="4" style="padding: 10px; text-align: center; background: #667eea;">Algoritmo PMPn</th>
        <th colspan="2" style="padding: 10px; text-align: center; background: #f59e0b;">Rango Primario</th>
        <th rowspan="2" style="padding: 10px; text-align: center; background: #10b981;">Tendencia</th>
        <th colspan="3" style="padding: 10px; text-align: center; background: #8b5cf6;">Separación</th>
        <th rowspan="2" style="padding: 10px; text-align: center; background: #ef4444; min-width: 100px;">Señal</th>
        <th rowspan="2" style="padding: 10px; text-align: center; background: #06b6d4;">IMP<sub>i</sub></th>
    </tr>
    <tr>
        <!-- Subencabezados WAM -->
        <th style="padding: 8px; background: #4a55c4;">WAM</th>
        <th style="padding: 8px; background: #4a55c4;">%</th>
        
        <!-- Subencabezados Algoritmo -->
        <th style="padding: 8px; background: #667eea;">PMP<sub>n</sub></th>
        <th style="padding: 8px; background: #667eea;">max(P)</th>
        <th style="padding: 8px; background: #667eea;">min(P)</th>
        <th style="padding: 8px; background: #667eea;">PMP<sub>n-1</sub></th>
        
        <!-- Subencabezados Rango -->
        <th style="padding: 8px; background: #f59e0b;">RP<sup>+</sup></th>
        <th style="padding: 8px; background: #f59e0b;">RP<sup>-</sup></th>
        
        <!-- Subencabezados Separación -->
        <th style="padding: 8px; background: #8b5cf6;">Dif<sub>n</sub></th>
        <th style="padding: 8px; background: #8b5cf6;">Prom</th>
        <th style="padding: 8px; background: #8b5cf6;">σ</th>
    </tr>
</thead>
```

### 4.4 Actualizar JavaScript para Cargar Datos con Algoritmo

```javascript
// Nueva función para cargar con algoritmo
async function loadPriceHistoryWithAlgorithm() {
    const symbol = document.getElementById('symbolDropdown').value;
    const dateFilter = document.getElementById('dateFilter').value;
    const wamPeriod = document.getElementById('wamPeriodDropdown').value;
    const enableAlgorithm = document.getElementById('enableAlgorithm').checked;
    
    if (!symbol || !dateFilter) {
        alert('Por favor selecciona símbolo y fecha');
        return;
    }
    
    showLoading();
    const btnLoad = document.getElementById('btnLoadHistory');
    btnLoad.disabled = true;
    
    try {
        let url;
        let queryParams = `dateFilter=${dateFilter}&symbolStr=${encodeURIComponent(symbol)}&wamPeriod=${wamPeriod}`;
        
        if (enableAlgorithm) {
            // Obtener parámetros del algoritmo
            const pt = document.getElementById('paramPt').value;
            const pr = document.getElementById('paramPr').value;
            const sigma = document.getElementById('paramSigma').value;
            const mp = document.getElementById('paramMp').value;
            const fd = document.getElementById('paramFd').value;
            
            queryParams += `&pt=${pt}&pr=${pr}&sigma=${sigma}&mp=${mp}&fd=${fd}`;
            url = '@Url.Action("GetDatePriceHistoryWithAlgorithm", "HistoryMT")' + `?${queryParams}`;
        } else {
            url = '@Url.Action("GetDatePriceHistory", "HistoryMT")' + `?${queryParams}`;
        }
        
        const response = await fetch(url);
        
        if (response.ok) {
            const data = await response.json();
            
            if (data && data.length > 0) {
                // Dibujar gráfico
                const candles = data.map(d => ({
                    time: getUnixTimeStamp(d.timE_MTAPI || d.time),
                    open: parseFloat(d.open),
                    high: parseFloat(d.high),
                    low: parseFloat(d.low),
                    close: parseFloat(d.close)
                }));
                
                drawHistoryChart('historyChartContainer', candles);
                
                // Llenar tabla
                populateDataTableWithAlgorithm(data, enableAlgorithm);
                
                // Habilitar descarga
                document.getElementById('btnDownloadExcel').disabled = false;
            } else {
                alert('No hay datos disponibles');
            }
        } else {
            alert(`Error: ${response.status}`);
        }
    } catch (error) {
        alert('Error al cargar datos: ' + error.message);
    } finally {
        hideLoading();
        btnLoad.disabled = false;
    }
}

// Nueva función para poblar tabla con columnas de algoritmo
function populateDataTableWithAlgorithm(data, showAlgorithm) {
    const tableBody = document.getElementById('priceDataTableBody');
    const highlightSignals = document.getElementById('highlightSignalRows').checked;
    
    tableBody.innerHTML = '';
    
    data.forEach((item, index) => {
        const row = document.createElement('tr');
        
        // Resaltar filas con señal
        if (highlightSignals && showAlgorithm) {
            if (item.signal === "COMPRA") {
                row.style.backgroundColor = '#d4edda';
            } else if (item.signal === "VENTA") {
                row.style.backgroundColor = '#f8d7da';
            }
        }
        
        let html = `
            <td class="text-center fw-bold">${index + 1}</td>
            <td>${item.time || item.timE_MTAPI}</td>
            <td class="numeric">${parseFloat(item.open).toFixed(5)}</td>
            <td class="numeric">${parseFloat(item.high).toFixed(5)}</td>
            <td class="numeric">${parseFloat(item.low).toFixed(5)}</td>
            <td class="numeric">${parseFloat(item.close).toFixed(5)}</td>
            <td class="numeric" style="color: #5a67d8; font-weight: 600;">${parseFloat(item.calculatedWAM).toFixed(5)}</td>
            <td class="numeric" style="color: ${item.percentageDifference >= 0 ? '#28a745' : '#dc3545'};">
                ${item.percentageDifference >= 0 ? '+' : ''}${parseFloat(item.percentageDifference).toFixed(4)}%
            </td>
        `;
        
        if (showAlgorithm) {
            html += `
                <td class="numeric" style="color: #667eea; font-weight: 600;">${parseFloat(item.pmpN || 0).toFixed(5)}</td>
                <td class="numeric">${parseFloat(item.maxP || 0).toFixed(5)}</td>
                <td class="numeric">${parseFloat(item.minP || 0).toFixed(5)}</td>
                <td class="numeric">${item.previousPMPn ? parseFloat(item.previousPMPn).toFixed(5) : '—'}</td>
                <td class="numeric">${parseFloat(item.rpPlus || 0).toFixed(5)}</td>
                <td class="numeric">${parseFloat(item.rpMinus || 0).toFixed(5)}</td>
                <td class="text-center" style="font-weight: 600; ${getTendenciaStyle(item.tendencia)}">${getTendenciaIcon(item.tendencia)}</td>
                <td class="numeric">${parseFloat(item.difn || 0).toFixed(5)}</td>
                <td class="numeric">${item.promDifn ? parseFloat(item.promDifn).toFixed(5) : '—'}</td>
                <td class="numeric">${item.sigmaDifn ? parseFloat(item.sigmaDifn).toFixed(5) : '—'}</td>
                <td class="text-center">${getSignalBadge(item.signal)}</td>
                <td class="numeric">${item.importeAcumulacion ? '€' + parseFloat(item.importeAcumulacion).toFixed(2) : '—'}</td>
            `;
        }
        
        row.innerHTML = html;
        tableBody.appendChild(row);
    });
}

function getTendenciaIcon(tendencia) {
    if (tendencia === "ALZA") return "↗ ALZA";
    if (tendencia === "BAJA") return "↘ BAJA";
    return "→ NEUTRO";
}

function getTendenciaStyle(tendencia) {
    if (tendencia === "ALZA") return "color: #28a745;";
    if (tendencia === "BAJA") return "color: #dc3545;";
    return "color: #6c757d;";
}

function getSignalBadge(signal) {
    if (signal === "COMPRA") {
        return '<span style="background: #28a745; color: white; padding: 4px 10px; border-radius: 4px; font-weight: 700;">🟢 COMPRA</span>';
    }
    if (signal === "VENTA") {
        return '<span style="background: #dc3545; color: white; padding: 4px 10px; border-radius: 4px; font-weight: 700;">🔴 VENTA</span>';
    }
    return '<span style="background: #e2e3e5; color: #383d41; padding: 4px 10px; border-radius: 4px;">⚪ —</span>';
}

// Sincronizar período PMPn con WAM
document.getElementById('wamPeriodDropdown').addEventListener('change', function() {
    document.getElementById('pmpnPeriod').value = this.value;
});

// Habilitar/deshabilitar campos de algoritmo
document.getElementById('enableAlgorithm').addEventListener('change', function() {
    const fields = document.getElementById('algorithmFields');
    fields.style.opacity = this.checked ? '1' : '0.5';
    
    const inputs = fields.querySelectorAll('input:not([readonly])');
    inputs.forEach(input => input.disabled = !this.checked);
});

// Restaurar valores por defecto
function resetAlgorithmParams() {
    document.getElementById('paramPt').value = 0.30;
    document.getElementById('paramPr').value = 0.75;
    document.getElementById('paramSigma').value = 2.0;
    document.getElementById('paramMp').value = 1000;
    document.getElementById('paramFd').value = 0.95;
}

// Reemplazar llamada en botón
document.getElementById('btnLoadHistory').onclick = loadPriceHistoryWithAlgorithm;
```

---

## 🏗️ FASE 5: Tiempo Real SignalR (Backend Central)

**Duración estimada:** 12-16 horas (aumentada por refactoring necesario)  
**Prioridad:** CRÍTICA  
**Objetivo:** Implementar procesamiento en tiempo real en SATI API y transmitir a N frontends  

---

### 🚨 **ANÁLISIS DE SITUACIÓN ACTUAL**

Antes de implementar tiempo real, es **CRÍTICO** entender por qué el código actual no funcionará y qué debe refactorizarse.

#### **❌ PROBLEMA 1: Arquitectura Stateless (Sin Estado)**

**Situación actual en histórico:**
```csharp
// API Controller - Funciona para histórico pero NO para realtime
[HttpGet("GetDatePriceHistory")]
public async Task<IActionResult> GetDatePriceHistory(...)
{
    var rates = await _metaTrader.GetDatePriceHistoryAsync(...);
    return Ok(rates); // ← Responde y olvida (sin estado)
}
```

**Por qué funciona para histórico:**
- ✅ Usuario pide datos → API responde → Fin
- ✅ No hay conexión persistente
- ✅ Datos inmutables (histórico no cambia)

**Por qué NO funcionará para realtime:**
- ❌ No hay forma de "empujar" datos al cliente
- ❌ Polling cada segundo = ineficiente (CPU, red, latencia)
- ❌ No escala (10 usuarios = 10 requests/segundo = colapso)

**✅ Solución requerida:** SignalR con conexión WebSocket persistente

---

#### **❌ PROBLEMA 2: Cálculos Completos en Cada Request**

**Situación actual:**
```csharp
[HttpGet("GetDatePriceHistoryWithAlgorithm")]
public async Task<IActionResult> GetDatePriceHistoryWithAlgorithm(...)
{
    // 1. Obtener 1440 registros (día completo)
    var rates = await _metaTrader.GetDatePriceHistoryAsync(...);
    
    // 2. Calcular algoritmo COMPLETO (1440 iteraciones)
    rates = _operativeAlgorithmSvc.CalculateBatch(rates, wamPeriod, parameters);
    
    return Ok(rates); // ← Cada petición = cálculo completo
}
```

**Por qué funciona para histórico:**
- ✅ Usuario pide una vez por sesión
- ✅ Puede tardar 2-3 segundos (aceptable para histórico)
- ✅ Datos no cambian

**Por qué será DESASTRE para realtime:**
```csharp
// ❌ Llega tick cada 0.5-2 segundos...
public void OnNewTick(Quote newQuote)
{
    var allRates = GetLast1440Rates(); // ← 1440 registros cada vez
    var calculated = CalculateBatch(allRates, ...); // ← O(n²) por tick
    // CPU al 100%, servidor colapsa en minutos
}
```

**Impacto:**
- ❌ CPU al 100% constante
- ❌ Latencia de 2-3 segundos por tick
- ❌ No escala (5 símbolos = servidor muerto)

**✅ Solución requerida:** Cálculo incremental con estado en memoria (O(1) por tick)

---

#### **❌ PROBLEMA 3: Timestamps Inconsistentes**

**Situación actual (confusa):**

**Backend (C#):**
```csharp
public string Time { get; set; } = null!; // "2026.06.11 09:00:00" (¿hora del broker? ¿UTC? ¿local?)
```

**Frontend (JavaScript) - DOS métodos diferentes:**
```javascript
// Para histórico
GetOriginalTimestamp: (timeStr) => {
    return Date.UTC(year, month-1, day, hour, min, sec) / 1000; // Interpreta como UTC
}

// Para realtime
GetRealtimeTimestamp: (timeStr) => {
    return new Date(dateStr).getTime() / 1000; // Aplica zona horaria local
}
```

**Por qué NO funcionará para realtime:**
- ❌ ¿Cuál método usar? Confusión garantizada
- ❌ MT5 envía hora del broker (GMT+2, GMT+3, etc.)
- ❌ Gráfico mostrará horas incorrectas
- ❌ Diferentes usuarios verán diferentes horas

**✅ Solución requerida:** Backend SIEMPRE envía UTC (ISO 8601), frontend usa un solo método

---

#### **❌ PROBLEMA 4: Responsabilidades Mezcladas**

**Situación actual:**
```csharp
// Controller hace TODO
[HttpGet("GetDatePriceHistoryWithAlgorithm")]
public async Task<IActionResult> GetDatePriceHistoryWithAlgorithm(...)
{
    // 1. Obtener datos
    var rates = await _metaTrader.GetDatePriceHistoryAsync(...);
    
    // 2. Crear parámetros
    var parameters = new ParametersAlgorithmDto { Pt = pt ?? 0.30, ... };
    
    // 3. Validar parámetros
    if (!parameters.IsValid()) return BadRequest(...);
    
    // 4. Calcular algoritmo
    rates = _operativeAlgorithmSvc.CalculateBatch(rates, ...);
    
    // 5. Log
    _logger.LogInformation(...);
    
    return Ok(rates);
}
```

**Por qué NO funcionará para realtime:**
- ❌ No puedes reutilizar esta lógica desde SignalR Hub
- ❌ Tendrás código duplicado entre HTTP y WebSocket
- ❌ Testing imposible (depende de HTTP context)

**✅ Solución requerida:** Service Layer compartido entre HTTP y SignalR

---

### 🎯 **REFACTORING PREVIO OBLIGATORIO**

**IMPORTANTE:** Invertir 2-3 días en refactoring **ANTES** de tiempo real ahorrará semanas de problemas.

#### **PRIORIDAD 1 - Bloqueante (DEBE hacerse antes):**

**1. Crear IRealtimeService con Estado en Memoria**
```csharp
// Services/RealtimeService.cs - MANTIENE ESTADO
public class RealtimeService : IRealtimeService
{
    // ← Cache de datos calculados por símbolo
    private readonly Dictionary<string, AlgorithmState> _states = new();
    
    public class AlgorithmState
    {
        public Queue<RateDto> Last50Rates { get; set; } = new();
        public double LastPMPn { get; set; }
        public double LastMaxP { get; set; }
        public double LastMinP { get; set; }
        public List<double> Last50Diffs { get; set; } = new();
    }
    
    public OperativeAlgorithmDto CalculateIncremental(string symbol, Quote newQuote)
    {
        var state = _states.GetOrAdd(symbol, new AlgorithmState());
        
        // ✅ Solo calcular el NUEVO valor (O(1) en lugar de O(n²))
        state.Last50Rates.Enqueue(newQuote);
        if (state.Last50Rates.Count > 50) state.Last50Rates.Dequeue();
        
        var newPMPn = state.Last50Rates.Average(r => (r.High + r.Low) / 2);
        var difn = newQuote.Close - state.LastPMPn;
        
        state.LastPMPn = newPMPn;
        // ... cálculos incrementales
        
        return new OperativeAlgorithmDto { ... };
    }
}
```

**Beneficios:**
- ✅ CPU: De 100% a 5%
- ✅ Latencia: De 2000ms a <10ms por tick
- ✅ Escalabilidad: De 5 usuarios máx a 100+ usuarios

**2. Implementar SignalR Hub con Sistema de Suscripciones**
```csharp
// Hubs/MetaTraderHub.cs
public class MetaTraderHub : Hub
{
    private readonly IRealtimeService _realtimeService;
    
    public async Task SubscribeToSymbol(string symbol)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, symbol);
        _realtimeService.RegisterClient(symbol, Context.ConnectionId);
    }
    
    public async Task UnsubscribeFromSymbol(string symbol)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol);
        _realtimeService.UnregisterClient(symbol, Context.ConnectionId);
    }
}

// Backend mantiene conexión y envía updates automáticamente
public void OnNewPrice(string symbol, Quote quote)
{
    _hubContext.Clients.Group(symbol).SendAsync("ReceivePrice", quote);
}
```

**3. Estandarizar Timestamps en Backend (SIEMPRE UTC)**
```csharp
// DTOs/QuoteDto.cs
public class QuoteDto
{
    public string Symbol { get; set; }
    public DateTime TimestampUtc { get; set; }  // ← SIEMPRE UTC
    
    [JsonPropertyName("time")]
    public string TimeISO => TimestampUtc.ToString("O"); // "2026-06-11T09:00:00.000Z"
}

// Al recibir de MT5, convertir INMEDIATAMENTE a UTC
public async Task<QuoteDto> GetCurrentQuoteAsync(string symbol)
{
    var quote = await _terminal.GetQuoteAsync(symbol);
    
    // MT5 devuelve hora del broker (GMT+2, GMT+3, etc.)
    var brokerTime = DateTime.ParseExact(quote.Time, "yyyy.MM.dd HH:mm:ss", null);
    var utcTime = TimeZoneInfo.ConvertTimeToUtc(brokerTime, _brokerTimeZone);
    
    return new QuoteDto
    {
        Symbol = symbol,
        TimestampUtc = utcTime,  // ← Garantizado UTC
        Bid = quote.Bid,
        Ask = quote.Ask
    };
}
```

**Frontend simplificado:**
```javascript
// UN SOLO método para todo (histórico y realtime)
function parseTimestamp(isoString) {
    return new Date(isoString).getTime() / 1000; // JavaScript lo convierte a local automáticamente
}
```

---

#### **PRIORIDAD 2 - Facilita Desarrollo:**

**4. Separar Service Layer en API**
```csharp
// Controllers/MetaTraderController.cs - Solo enruta
[HttpGet("GetDatePriceHistoryWithAlgorithm")]
public async Task<IActionResult> GetDatePriceHistoryWithAlgorithm(...)
{
    var request = new AlgorithmHistoryRequest { ... };
    var result = await _historyService.GetHistoryWithAlgorithmAsync(request);
    
    return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
}

// Hubs/MetaTraderHub.cs - REUTILIZA el mismo servicio
public async Task GetHistoricalData(AlgorithmHistoryRequest request)
{
    var result = await _historyService.GetHistoryWithAlgorithmAsync(request); // ← MISMA lógica
    await Clients.Caller.SendAsync("ReceiveHistorical", result.Data);
}
```

---

### 📋 **CHECKLIST DE PREPARACIÓN PARA REALTIME**

#### **Antes de Empezar:**
- [ ] Leer y entender TODOS los problemas descritos arriba
- [ ] Confirmar disponibilidad de 2-3 días para refactoring
- [ ] Backup del código actual
- [ ] Crear branch específico para realtime

#### **Refactoring Obligatorio:**
- [ ] Crear `IRealtimeService` con estado en memoria
- [ ] Implementar cálculo incremental (O(1) por tick)
- [ ] Crear DTOs con timestamps UTC estandarizados
- [ ] Implementar conversión MT5 → UTC en repository
- [ ] Crear `IHistoryMTService` para reutilizar lógica
- [ ] Extraer lógica de controllers a servicios
- [ ] Crear tests unitarios para cálculo incremental
- [ ] Validar que incremental == batch (mismo resultado)

#### **Implementación SignalR:**
- [ ] Configurar SignalR en Program.cs
- [ ] Crear `MetaTraderHub` con métodos de suscripción
- [ ] Implementar sistema de grupos por símbolo
- [ ] Agregar reconexión automática en frontend

---

### 🛠️ **IMPLEMENTACIÓN PASO A PASO**

Ahora sí, con el refactoring completado, procedemos a implementar tiempo real:

### 5.1 Crear Servicio de Estado en Memoria (Refactorizado)  

**Archivo:** `/Sati-Net-Last.API/Services/RealtimeAlgorithmState.cs` (NUEVO)

```csharp
namespace Sati_Net_Last.API.Services;

/// <summary>
/// Estado en memoria para cálculo incremental de algoritmo operativo
/// Mantiene historial necesario para calcular cada tick sin recalcular todo
/// </summary>
public class RealtimeAlgorithmState
{
    public string Symbol { get; set; }
    public int Period { get; set; }
    public DateTime LastUpdate { get; set; }
    
    // Parámetros del algoritmo
    public ParametersAlgorithmDto Parameters { get; set; }
    
    // Cola de precios recientes (ventana móvil para PMPn)
    public Queue<double> RecentCloses { get; set; } = new();
    
    // Estado de extremos actuales
    public double CurrentMaxP { get; set; }
    public double CurrentMinP { get; set; }
    public double CurrentPMPn { get; set; }
    public double? PreviousPMPn { get; set; }
    
    // Historial de Difn para estadísticas
    public List<double> DifnHistory { get; set; } = new();
    
    // Contador de operaciones en tendencia actual
    public int OperationCounter { get; set; }
    public string CurrentTrend { get; set; } = "NEUTRO";
    
    public RealtimeAlgorithmState(string symbol, int period, ParametersAlgorithmDto parameters)
    {
        Symbol = symbol;
        Period = period;
        Parameters = parameters ?? new ParametersAlgorithmDto();
        LastUpdate = DateTime.UtcNow; // ← Siempre UTC
    }
    
    /// <summary>
    /// Agregar nuevo precio a la ventana móvil
    /// </summary>
    public void AddPrice(double closePrice)
    {
        RecentCloses.Enqueue(closePrice);
        if (RecentCloses.Count > Period + 1)
        {
            RecentCloses.Dequeue(); // Mantener solo últimos n+1
        }
        LastUpdate = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Resetear estado (útil al inicio del día)
    /// </summary>
    public void Reset()
    {
        RecentCloses.Clear();
        DifnHistory.Clear();
        OperationCounter = 0;
        CurrentTrend = "NEUTRO";
        PreviousPMPn = null;
        CurrentMaxP = 0;
        CurrentMinP = 0;
    }
}
```

### 5.2 Crear Servicio de Tiempo Real (Cálculo Incremental)

**Archivo:** `/Sati-Net-Last.API/Services/RealtimeAlgorithmService.cs` (NUEVO)

```csharp
namespace Sati_Net_Last.API.Services;

/// <summary>
/// Servicio de cálculo incremental para tiempo real
/// Mantiene estado en memoria y calcula O(1) por tick (no O(n²))
/// </summary>
public class RealtimeAlgorithmService
{
    private readonly ILogger<RealtimeAlgorithmService> _logger;
    private readonly OperativeAlgorithmSvc _batchService; // Para inicialización
    
    // Cache de estados por símbolo_período
    private readonly ConcurrentDictionary<string, RealtimeAlgorithmState> _states = new();
    
    public RealtimeAlgorithmService(
        ILogger<RealtimeAlgorithmService> logger,
        OperativeAlgorithmSvc batchService)
    {
        _logger = logger;
        _batchService = batchService;
    }
    
    /// <summary>
    /// Inicializar estado desde histórico del día
    /// </summary>
    public void InitializeFromHistory(string symbol, List<Rates> historicalRates, 
                                      int period, ParametersAlgorithmDto parameters)
    {
        if (historicalRates == null || historicalRates.Count == 0)
        {
            _logger.LogWarning($"No historical data for {symbol}");
            return;
        }
        
        string stateKey = $"{symbol}_{period}";
        var state = new RealtimeAlgorithmState(symbol, period, parameters);
        
        // Tomar últimos n precios para ventana móvil
        var recentPrices = historicalRates
            .TakeLast(period + 1)
            .Select(r => r.CLOSE)
            .ToList();
        
        foreach (var price in recentPrices)
        {
            state.AddPrice(price);
        }
        
        // Inicializar con último estado conocido
        var lastRate = historicalRates.Last();
        state.CurrentPMPn = lastRate.PMPn;
        state.PreviousPMPn = lastRate.PreviousPMPn;
        state.CurrentMaxP = lastRate.MaxP;
        state.CurrentMinP = lastRate.MinP;
        state.CurrentTrend = lastRate.Tendencia;
        
        // Copiar historial de Difn
        state.DifnHistory = historicalRates
            .Where(r => r.Difn > 0)
            .Select(r => r.Difn)
            .ToList();
        
        _states[stateKey] = state;
        
        _logger.LogInformation($"✅ Estado inicializado para {stateKey}: " +
            $"{state.RecentCloses.Count} precios, {state.DifnHistory.Count} Difn");
    }
    
    /// <summary>
    /// Calcular algoritmo incrementalmente para nuevo tick (O(1))
    /// </summary>
    public OperativeAlgorithmDto CalculateIncremental(string symbol, Quote newQuote, 
                                                      int period, ParametersAlgorithmDto parameters)
    {
        string stateKey = $"{symbol}_{period}";
        
        // Obtener o crear estado
        var state = _states.GetOrAdd(stateKey, _ => 
            new RealtimeAlgorithmState(symbol, period, parameters));
        
        // Agregar nuevo precio
        state.AddPrice(newQuote.Close);
        
        // ✅ Calcular PMPn solo con precios en memoria (O(1))
        double newPMPn = CalculatePMPnIncremental(state.RecentCloses.ToList(), period);
        
        // Primera vez: inicializar extremos
        if (state.PreviousPMPn == null)
        {
            state.CurrentMaxP = newPMPn;
            state.CurrentMinP = newPMPn;
        }
        
        // Actualizar extremos
        if (newPMPn >= state.CurrentMaxP) state.CurrentMaxP = newPMPn;
        if (newPMPn <= state.CurrentMinP) state.CurrentMinP = newPMPn;
        
        // Calcular rangos primarios
        double rpPlus = Math.Abs(newPMPn - state.CurrentMinP);
        double rpMinus = Math.Abs(state.CurrentMaxP - newPMPn);
        
        // Detectar tendencia
        string tendencia = DetectTrend(rpPlus, rpMinus, parameters, 
                                       ref state.CurrentMaxP, ref state.CurrentMinP, 
                                       newPMPn, ref state.OperationCounter);
        
        // Calcular Difn
        double difn = Math.Abs(newPMPn - newQuote.Close);
        state.DifnHistory.Add(difn);
        
        // Mantener solo últimos 200 Difn para estadísticas
        if (state.DifnHistory.Count > 200)
        {
            state.DifnHistory.RemoveAt(0);
        }
        
        // Calcular estadísticas de Difn
        double? promDifn = null;
        double? sigmaDifn = null;
        
        if (state.DifnHistory.Count > 1)
        {
            promDifn = state.DifnHistory.Average();
            sigmaDifn = CalculateStdDev(state.DifnHistory);
        }
        
        // Evaluar señal
        string signal = EvaluateSignal(tendencia, newPMPn, state.CurrentMaxP, state.CurrentMinP,
                                       state.PreviousPMPn, difn, promDifn, sigmaDifn, 
                                       parameters.Sigma);
        
        // Calcular importe
        double? importeAcumulacion = null;
        if (signal != "NINGUNA")
        {
            importeAcumulacion = CalculateAmount(state.OperationCounter, parameters.Mp, parameters.Fd);
            state.OperationCounter++;
        }
        
        // Actualizar estado para próximo tick
        state.PreviousPMPn = state.CurrentPMPn;
        state.CurrentPMPn = newPMPn;
        state.CurrentTrend = tendencia;
        
        // Construir DTO con todos los valores
        return new OperativeAlgorithmDto
        {
            Time = newQuote.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss"),
            Open = newQuote.Open,
            High = newQuote.High,
            Low = newQuote.Low,
            Close = newQuote.Close,
            CalculatedWAM = 0, // WAM se calcula por separado si es necesario
            PercentageDifference = 0,
            PMPn = newPMPn,
            MaxP = state.CurrentMaxP,
            MinP = state.CurrentMinP,
            PreviousPMPn = state.PreviousPMPn,
            RPPlus = rpPlus,
            RPMinus = rpMinus,
            Tendencia = tendencia,
            Difn = difn,
            PromDifn = promDifn,
            SigmaDifn = sigmaDifn,
            Signal = signal,
            ImporteAcumulacion = importeAcumulacion
        };
    }
    
    // Métodos privados de cálculo (similares a OperativeAlgorithmSvc pero optimizados)
    private double CalculatePMPnIncremental(List<double> recentCloses, int period) { /* ... */ }
    private string DetectTrend(...) { /* ... */ }
    private string EvaluateSignal(...) { /* ... */ }
    private double CalculateAmount(int operationNumber, double mp, double fd) { /* ... */ }
    private double CalculateStdDev(List<double> values) { /* ... */ }
    
    /// <summary>
    /// Limpiar estado de un símbolo
    /// </summary>
    public void ClearState(string symbol, int period)
    {
        string stateKey = $"{symbol}_{period}";
        _states.TryRemove(stateKey, out _);
        _logger.LogInformation($"Estado limpiado para {stateKey}");
    }
}
```

### 5.3 Crear Servicio Background para Tiempo Real

**Archivo:** `/Sati-Net-Last.API/Services/RealTimeAlgoritmoHostedService.cs` (NUEVO)

```csharp
namespace Sati_Net_Last.API.Services;

/// <summary>
/// 🎯 SERVICIO CENTRAL DE TIEMPO REAL
/// Escucha eventos de MT API, calcula algoritmo UNA SOLA VEZ,
/// guarda en DB y transmite via SignalR a TODOS los frontends conectados
/// </summary>
public class RealTimeAlgoritmoHostedService : BackgroundService
{
    private readonly ILogger<RealTimeAlgoritmoHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly OperativeAlgorithmSvc _algoritmoService;
    private readonly IHubContext<MetaTraderHub> _hubContext;
    
    // Símbolos activos para monitorear (configurables)
    private HashSet<string> _activeSymbols = new HashSet<string>();
    
    public RealTimeAlgoritmoHostedService(
        ILogger<RealTimeAlgoritmoHostedService> logger,
        IServiceProvider serviceProvider,
        OperativeAlgorithmSvc algoritmoService,
        IHubContext<MetaTraderHub> hubContext)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _algoritmoService = algoritmoService;
        _hubContext = hubContext;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de Tiempo Real iniciado");
        
        // Inicialización al comenzar el día
        await InitializeDailyState();
        
        // Loop principal
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Verificar si cambió el día (reinicializar estados)
                await CheckDayChange();
                
                // Esperar 1 segundo
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en servicio de tiempo real");
            }
        }
        
        _logger.LogInformation("Servicio de Tiempo Real detenido");
    }
    
    private async Task InitializeDailyState()
    {
        _logger.LogInformation("Inicializando estados desde histórico del día actual");
        
        // TODO: Cargar histórico del día para cada símbolo activo
        // y llamar a _algoritmoService.InitializeFromHistory()
    }
    
    private async Task CheckDayChange()
    {
        // TODO: Implementar detección de cambio de día
        // y reinicializar estados
    }
    
    /// <summary>
    /// Procesar nueva vela en tiempo real
    /// </summary>
    public async Task ProcessNewCandle(string symbol, Rates newRate, int period)
    {
        try
        {
            // Parámetros por defecto (podrían venir de configuración)
            var parameters = new AlgoritmoParametersDto
            {
                Period = period,
                Pt = 0.30,
                Pr = 0.75,
                Sigma = 2.0,
                Mp = 1000,
                Fd = 0.95
            };
            
            // Calcular algoritmo incrementalmente
            newRate = _algoritmoService.CalculateIncremental(symbol, newRate, period, parameters);
            
            // 📡 BROADCAST a TODOS los frontends conectados via SignalR
            // Un solo cálculo → N clientes reciben la misma actualización
            await _hubContext.Clients.All.SendAsync("ReceiveRealTimeUpdate", new
            {
                Symbol = symbol,
                Time = newRate.TIME,
                Data = newRate
            });
            
            // También guardar en DB para persistencia
            using (var scope = _serviceProvider.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ITerminalRepo>();
                await repo.SaveRateAsync(newRate); // Persistir para histórico
            }
            
            // Si hay señal, enviar notificación especial
            if (newRate.Signal != null && newRate.Signal != "NINGUNA")
            {
                await _hubContext.Clients.All.SendAsync("ReceiveTradeSignal", new
                {
                    Symbol = symbol,
                    Signal = newRate.Signal,
                    Time = newRate.TIME,
                    Price = newRate.CLOSE,
                    PMPn = newRate.PMPn,
                    ImporteAcumulacion = newRate.ImporteAcumulacion
                });
                
                _logger.LogInformation($"🎯 SEÑAL {newRate.Signal}: {symbol} @ {newRate.CLOSE}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error procesando vela de {symbol}");
        }
    }
}
```

### 5.2 Modificar MTRepo para Conectar Eventos

**Archivo:** `/Sati-Net-Last.API/MTRepositories/MTRepo.cs`

```csharp
// Agregar inyección de dependencia
private readonly RealTimeAlgoritmoHostedService _realtimeService;

public MTRepo(
    IHubContext<MetaTraderHub> mtHubContext,
    ITerminalRepo terminalRepo,
    SatiDevContext satiDevContext,
    ILogger<MTRepo> logger,
    RealTimeAlgoritmoHostedService realtimeService) // NUEVO
{
    // ...
    _realtimeService = realtimeService;
}

public async Task ConnectToMetaTrader()
{
    // ..código existente...
    
    // DESCOMENTAR Y MEJORAR:
    _terminal.OnOHLC += Mt5_OnOHLC; // Activar evento de velas
}

// NUEVO MÉTODO
private async void Mt5_OnOHLC(object? sender, OHLC_Msg e)
{
    try
    {
        _logger.LogInformation($"Nueva vela OHLC: {e.SYMBOL} - {e.PERIOD}");
        
        if (e.OHLC != null && e.OHLC.Count > 0)
        {
            foreach (var rate in e.OHLC)
            {
                // Determinar período (M1 = 50 por defecto, configurable)
                int period = 50;
                
                // Procesar vela en tiempo real
                await _realtimeService.ProcessNewCandle(e.SYMBOL, rate, period);
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error en Mt5_OnOHLC");
    }
}
```

### 5.3 Registrar Servicio Background

**Archivo:** `/Sati-Net-Last.API/Program.cs`

```csharp
// Agregar después de AddSingleton<OperativeAlgorithmSvc>():

// Registrar servicio de tiempo real como HostedService
builder.Services.AddSingleton<RealTimeAlgoritmoHostedService>();
builder.Services.AddHostedService(provider => 
    provider.GetRequiredService<RealTimeAlgoritmoHostedService>());
```

### 5.4 Frontend - Consumir Datos en Tiempo Real

**Importante:** Los frontends solo CONSUMEN los datos que SATI API ya procesó.

**Archivo:** `/Sati-Net-Last.Web/Views/SatiTrader/Index.cshtml`

Agregar cliente SignalR para recibir actualizaciones del backend:

```javascript
// 🔌 CLIENTE FRONTEND - Conectar al Hub de SATI API
// Este frontend solo recibe datos ya procesados por el backend
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/metatraderhub") // URL relativa o absoluta según configuración
    .withAutomaticReconnect([0, 2000, 10000, 30000]) // Reconexión automática
    .configureLogging(signalR.LogLevel.Information)
    .build();

// 📥 Recibir actualizaciones en tiempo real del backend
connection.on("ReceiveRealTimeUpdate", (data) => {
    console.log("📊 Nueva vela procesada por SATI API:", data);
    
    // Solo actualizar UI - NO recalcular nada
    updateRealTimeDisplay(data);
    appendRowToTable(data);
    updateChart(data);
});

// 🎯 Recibir señales de trading
connection.on("ReceiveTradeSignal", (signal) => {
    console.log("🎯 SEÑAL DE TRADING:", signal);
    
    // Notificación al usuario
    showTradeSignalNotification(signal);
    playNotificationSound();
    
    // Highlight en la tabla
    highlightSignalRow(signal);
});

// 🔄 Manejo de reconexión
connection.onreconnecting((error) => {
    console.warn("⚠️ Reconectando a SATI API...", error);
    showConnectionStatus("Reconectando...");
});

connection.onreconnected((connectionId) => {
    console.log("✅ Reconectado a SATI API", connectionId);
    showConnectionStatus("Conectado");
    // Solicitar datos faltantes durante desconexión
    requestMissedData();
});

connection.onclose((error) => {
    console.error("❌ Desconectado de SATI API", error);
    showConnectionStatus("Desconectado");
});

// 🚀 Iniciar conexión
connection.start()
    .then(() => {
        console.log("✅ Conectado a SATI API SignalR Hub");
        showConnectionStatus("Conectado");
    })
    .catch(err => {
        console.error("❌ Error conectando a SATI API:", err);
        showConnectionStatus("Error de conexión");
    });

// 📊 Función ejemplo para actualizar UI
function updateRealTimeDisplay(data) {
    // Agregar nueva fila a la tabla
    const row = `
        <tr class="${data.Data.Signal !== 'NINGUNA' ? 'table-warning' : ''}">
            <td>${data.Time}</td>
            <td>${data.Data.Close.toFixed(5)}</td>
            <td>${data.Data.PMPn.toFixed(5)}</td>
            <td>${data.Data.Signal}</td>
            <!-- más columnas -->
        </tr>
    `;
    $('#realTimeTable tbody').prepend(row);
    
    // Limitar filas visibles (opcional)
    const maxRows = 100;
    $('#realTimeTable tbody tr').slice(maxRows).remove();
}
```

---

## 📊 FASE 6: Testing y Validación

**Duración estimada:** 4-6 horas  
**Prioridad:** ALTA  

### 6.1 Tests Unitarios para Algoritmo

**Archivo:** `/Sati-Net-Last.Tests/OperativeAlgorithmSvcTests.cs` (NUEVO)

```csharp
[Fact]
public void CalculatePMPn_ShouldReturnCorrectValue()
{
    // Arrange
    var service = new OperativeAlgorithmSvc(Mock.Of<ILogger<OperativeAlgorithmSvc>>());
    var rates = CreateTestRates();
    var parameters = new AlgoritmoParametersDto { Period = 20 };
    
    // Act
    var result = service.CalculateBatch(rates, 20, parameters);
    
    // Assert
    Assert.NotNull(result);
    Assert.True(result[0].PMPn > 0);
}

[Fact]
public void DetectarTendencia_ALZA_ShouldReturnCorrectSignal()
{
    // Test para tendencias...
}

[Fact]
public void EvaluarSenal_ConCuatroCondiciones_ShouldReturnCOMPRA()
{
    // Test para señales...
}
```

### 6.2 Casos de Prueba

1. **Prueba de cálculo PMPn:**
   - Comparar con valores de Excel existentes
   - Verificar ventana móvil funciona correctamente

2. **Prueba de detección de tendencia:**
   - Simular escenarios de alza/baja/neutro
   - Verificar rompimientos

3. **Prueba de señales:**
   - Verificar las 4 condiciones
   - Validar que solo se emiten cuando todas se cumplen

4. **Prueba de cálculo incremental:**
   - Comparar resultado batch vs incremental
   - Deben ser idénticos

5. **Prueba de performance:**
   - Medir tiempo de cálculo para 1440 registros (día completo)
   - Objetivo: < 100ms

---

## 📈 FASE 7: Optimizaciones y Mejoras Finales

**Duración estimada:** 4-6 horas  
**Prioridad:** BAJA  

### 7.1 Optimizaciones

1. **Cache de resultados**
   - Guardar cálculos en base de datos
   - Evitar recalcular histórico conocido

2. **Paralelización**
   - Calcular múltiples símbolos en paralelo
   - Usar Task.WhenAll()

3. **Compresión de datos**
   - Reducir tamaño de respuestas JSON
   - Usar gzip

### 7.2 Mejoras de UX

1. **Indicadores visuales**
   - Animaciones al recibir señales
   - Sonidos configurables

2. **Dashboard de resumen**
   - Total de señales del día
   - Tasa de acierto
   - Rendimiento estimado

3. **Exportación avanzada**
   - PDF con gráficos
   - CSV con todos los datos

---

## 🗓️ CRONOGRAMA ESTIMADO

| Fase | Duración | Dependencias | Prioridad |
|------|----------|--------------|-----------|
| Fase 1: Modelos y DTOs | 4-6 horas | Ninguna | ALTA |
| Fase 2: Servicio Algoritmo | 8-12 horas | Fase 1 | ALTA |
| Fase 3: Backend API | 4-6 horas | Fase 2 | ALTA |
| Fase 4: Frontend Web | 6-8 horas | Fase 3 | ALTA |
| **Fase 5: Tiempo Real + Refactoring** | **12-16 horas** | Fases 2,3,4 | **CRÍTICA** |
| Fase 6: Testing | 4-6 horas | Todas anteriores | ALTA |
| Fase 7: Optimizaciones | 4-6 horas | Todas anteriores | BAJA |

**TOTAL ESTIMADO:** 42-60 horas (5-8 días de trabajo)

**⚠️ NOTA IMPORTANTE:** La Fase 5 requiere más tiempo debido al refactoring obligatorio antes de implementar realtime. Este tiempo extra (4-6 horas adicionales) ahorrará semanas de problemas futuros y evitará colapsos de performance.

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

### Fase 1: Preparación
- [ ] Crear AlgoritmoOperativoDto.cs
- [ ] Crear AlgoritmoParametersDto.cs
- [ ] Extender Rates.cs con campos nuevos
- [ ] Compilar y verificar sin errores

### Fase 2: Servicio Core
- [ ] Crear AlgoritmoState.cs
- [ ] Crear OperativeAlgorithmSvc.cs
- [ ] Implementar CalculateBatch()
- [ ] Implementar CalculateIncremental()
- [ ] Implementar métodos privados de cálculo
- [ ] Probar manualmente con datos de prueba

### Fase 3: API Backend
- [ ] Registrar servicio en Program.cs
- [ ] Crear endpoint GetDatePriceHistoryWithAlgorithm
- [ ] Probar endpoint con Postman
- [ ] Verificar respuesta JSON correcta

### Fase 4: Frontend
- [ ] Actualizar controlador web
- [ ] Agregar sección de parámetros en vista
- [ ] Agregar checkboxes de visualización
- [ ] Actualizar tabla con nuevas columnas
- [ ] Agregar JavaScript para cargar datos
- [ ] Probar interfaz completa

### Fase 5: Tiempo Real + Refactoring
**Refactoring Previo (Prioridad CRÍTICA):**
- [ ] Leer y entender análisis de problemas en Fase 5
- [ ] Crear branch específico para refactoring
- [ ] Crear `RealtimeAlgorithmState.cs` (estado en memoria)
- [ ] Crear `RealtimeAlgorithmService.cs` (cálculo incremental O(1))
- [ ] Implementar conversión MT5 → UTC en MTRepo
- [ ] Crear DTOs con `TimestampUtc` (siempre UTC)
- [ ] Modificar frontend para usar UN solo método de timestamps
- [ ] Extraer Service Layer (`IHistoryMTService`)
- [ ] Tests unitarios: Validar incremental == batch
- [ ] Probar cálculo incremental con datos reales

**Implementación SignalR:**
- [ ] Configurar SignalR en Program.cs
- [ ] Crear `MetaTraderHub.cs` con suscripciones
- [ ] Implementar sistema de grupos por símbolo
- [ ] Crear `RealTimeAlgoritmoHostedService`
- [ ] Modificar MTRepo para conectar eventos OnOHLC
- [ ] Registrar hosted service
- [ ] Frontend: Agregar cliente SignalR con reconexión
- [ ] Probar conexión WebSocket
- [ ] Verificar flujo de datos en tiempo real (1 símbolo)
- [ ] Validar latencia < 100ms por tick
- [ ] Probar con múltiples símbolos simultáneos

### Fase 6: Testing
- [ ] Crear pruebas unitarias
- [ ] Ejecutar casos de prueba
- [ ] Validar con datos reales
- [ ] Corregir bugs encontrados

### Fase 7: Pulido
- [ ] Implementar optimizaciones
- [ ] Mejorar experiencia de usuario
- [ ] Documentar código
- [ ] Crear manual de usuario

---

## 🎯 HITOS CLAVE

1. **Hito 1 (Fase 1-2):** Servicio de algoritmo funcionando en aislamiento
2. **Hito 2 (Fase 3-4):** Interfaz histórica completa con algoritmo
3. **Hito 3 (Fase 5):** Tiempo real funcionando con 1 símbolo
4. **Hito 4 (Fase 6-7):** Sistema completo, testeado y optimizado

---

## ⚠️ RIESGOS Y MITIGACIONES

| Riesgo | Impacto | Probabilidad | Mitigación |
|--------|---------|--------------|------------|
| Performance lenta con muchos datos | Alto | Media | Implementar paginación, cálculo incremental |
| Inconsistencia batch vs incremental | Alto | Baja | Tests exhaustivos de comparación |
| Pérdida de estado en memoria | Medio | Media | Persistencia periódica en DB |
| Eventos MT API no se disparan | Alto | Baja | Validar suscripciones, logs detallados |
| SignalR se desconecta | Medio | Media | Reconexión automática implementada |
| **CPU al 100% en realtime sin refactoring** | **CRÍTICO** | **Alta** | **Cálculo incremental obligatorio (O(1) vs O(n²))** |
| **Memory leaks por estado no gestionado** | **Alto** | **Media** | **ConcurrentDictionary con límites y limpieza periódica** |
| **Timestamps inconsistentes causan bugs** | **Alto** | **Alta** | **Estandarización UTC en backend desde el inicio** |
| **Código duplicado HTTP vs SignalR** | **Medio** | **Alta** | **Service Layer compartido desde Fase 5** |

---

## ⏱️ COMPARATIVA DE TIEMPO: CON vs SIN REFACTORING

### **Sin Refactoring Previo (NO RECOMENDADO):**
```
Fase 5 (implementación directa):     5-7 días
Debugging problemas performance:     8-12 días  ← CPU 100%, memory leaks
Debugging timestamps incorrectos:    3-5 días   ← Gráficos muestran horas mal
Refactoring forzado bajo presión:   5-7 días   ← Código ya en producción
──────────────────────────────────────────────
TOTAL:                               21-31 días (4-6 semanas)
Estado final:                        Código técnico difícil de mantener
```

### **Con Refactoring Previo (RECOMENDADO):**
```
Fase 5 (refactoring + implementación): 12-16 horas (2 días)
Testing y ajustes:                      4-6 horas (1 día)
──────────────────────────────────────────────
TOTAL:                                  16-22 horas (2-3 días)
Estado final:                           Código limpio, escalable, mantenible
```

**💰 ROI del Refactoring:** Invertir 2-3 días ahorra 18-28 días de problemas futuros.

---

## 📝 NOTAS IMPORTANTES

1. **Mantener WAM existente:** No modificar cálculo actual, solo agregar nuevo
2. **Compatibilidad retroactiva:** Endpoints antiguos deben seguir funcionando
3. **Configuración flexible:** Todos los parámetros deben ser ajustables
4. **Logging extensivo:** Para debugging, especialmente en tiempo real
5. **Validación de datos:** Verificar que MT API envía datos correctos
6. **⚠️ CRÍTICO:** No implementar realtime sin el refactoring descrito en Fase 5
7. **Timestamps SIEMPRE UTC:** Backend convierte MT5→UTC, frontend solo renderiza
8. **Estado en memoria limitado:** Máximo 200 registros por símbolo en cache
9. **Monitoreo de performance:** CPU debe mantenerse < 10% en realtime

---

## 🚀 PRÓXIMOS PASOS RECOMENDADOS

1. ✅ **Revisar y aprobar este plan**
2. ✅ **Comenzar con Fase 1** (modelos básicos)
3. ✅ **Implementar Fase 2** (lógica core del algoritmo)
4. ✅ **Probar algoritmo en aislamiento** con datos de prueba
5. ✅ **Continuar con Fase 3-4** (integración API y frontend)
6. ✅ **Validar con usuario final** antes de tiempo real
7. ✅ **Implementar Fase 5** (tiempo real)
8. ✅ **Testing exhaustivo**

---

**¿Apruebas este plan? ¿Algún ajuste o comentario antes de comenzar la implementación?**
