using System.Collections;
using System.Threading.Tasks;
using Sati_Models.DBModels;
using Microsoft.AspNetCore.SignalR;
using MTsocketAPI.MT5;
using Sati_Net_Last.API.Hubs;
using Sati_Net_Last.API.MTRepositories.Interfaces;
using OfficeOpenXml; // Asegúrate de tener EPPlus instalado
using OfficeOpenXml.Style;
using Sati_Net_Last.API.Data;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Sati_Net_Last.API.MTRepositories;

public class MTRepo : IMTRepo
{
    private readonly IHubContext<MetaTraderHub> _mtHubContext;
    private readonly SatiDevContext _satiDevContext;
    private readonly ILogger<MTRepo> _logger; // ✅ Agregar ILogger
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly object _trackingLock = new();
    private readonly HashSet<string> _trackedSymbols = new(StringComparer.OrdinalIgnoreCase);
    private readonly TerminalRepo _terminalRepo;

    public MTRepo
    (
        IHubContext<MetaTraderHub> mtHubContext,
        SatiDevContext satiDevContext,
        TerminalRepo terminalRepo,
        ILogger<MTRepo> logger // ✅ Inyectar logger
    )
    {
        _mtHubContext = mtHubContext;
        _satiDevContext = satiDevContext;
        _terminalRepo = terminalRepo;
        _logger = logger;
    }

    public async Task ConnectToMetaTrader()
    {
        await _connectLock.WaitAsync();
        try
        {
            if (_terminalRepo.IsConnected)
                return;

            _logger.LogInformation("Attempting to connect to MetaTrader...");
            _logger.LogInformation(_terminalRepo.GetTerminal() == null ? "Terminal is null." : "Terminal instance exists.");
            // if (_terminalRepo.GetTerminal() != null)
            // {
            //     _logger.LogInformation("Already connected to MetaTrader.");
            //     return;
            // }

            // _terminal = new Terminal();
            // _terminal.OnPrice += Mt5_OnPrice;
            // _terminal.Connect();

            var connected = _terminalRepo.Connect();
            if (!connected)
            {
                _logger.LogError("Could not connect to MTsocketAPI.");
                return;
            }

            _terminalRepo.GetTerminal().OnPrice += Mt5_OnPrice;

            _logger.LogInformation("MTsocketAPI connected. Waiting for the administrator to select symbols to track.");

            // if (_terminal.Connect())
            // {
            //     await _mtHubContext.Clients.All.SendAsync("ReceiveMetaTraderLogin", "Connected to MetaTrader successfully.");
            //     _logger.LogInformation(_terminal.GetSymbolList().Where(x => x.TRADE_MODE != 0).Select(x => x.NAME).ToList());
            //     _terminal.TrackPrices(new List<string>() { "EURUSD" });
            // }

            // if (mt5 != null)
            // {
            //     var rates = mt5.PriceHistory(cmbSymbols.Text, tf, DateTime.Now.AddHours(-12).AddMinutes(-TimeSpanFromTF(tf).TotalMinutes * 30), DateTime.Now.AddDays(1));

            //     foreach (var item in rates)
            //     {
            //         ScottPlot.OHLC candle = new OHLC(open: item.OPEN, high: item.HIGH, low: item.LOW, close: item.CLOSE, Convert.ToDateTime(item.TIME), TimeSpanFromTF(tf));
            //         prices.Add(candle);
            //     }

            //     prices.Reverse();

            //     fnplot = formsPlot1.Plot.AddCandlesticks(prices.ToArray());
            //     fnplot.YAxisIndex = formsPlot1.Plot.RightAxis.AxisIndex;

            //     mt5.TrackPrices(new List<string>() { cmbSymbols.Text });

            //     formsPlot1.Plot.AxisAuto();
            //     formsPlot1.Refresh();
            // }
        }
        catch (Exception ex)
        {
            _logger.LogError("Please check that MTsocketAPI is running. \nError: " + ex.Message);
            // Application.Exit();
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async Task TrackSymbolsAsync(List<string> symbols)
    {
        await ConnectToMetaTrader();

        var normalizedSymbols = symbols?
            .Where(symbol => !string.IsNullOrWhiteSpace(symbol))
            .Select(NormalizeSymbol)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (!_terminalRepo.IsConnected)
            throw new InvalidOperationException("MetaTrader no está conectado. El administrador debe conectar MT5 primero.");

        List<string> symbolsToTrack;

        if (normalizedSymbols.Count == 0)
        {
            lock (_trackingLock)
            {
                _trackedSymbols.Clear();
            }

            _logger.LogInformation("Tracking cleared. No symbols selected by the administrator.");
            return;
        }

        lock (_trackingLock)
        {
            _trackedSymbols.Clear();
            foreach (var symbol in normalizedSymbols)
                _trackedSymbols.Add(symbol);

            symbolsToTrack = _trackedSymbols.ToList();
        }

        _terminalRepo.TrackPrices(symbolsToTrack);
        _logger.LogInformation("Tracking symbols active: {Symbols}", string.Join(", ", symbolsToTrack));
        return;
    }

    public async Task EnsureSymbolTrackedAsync(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("El símbolo es obligatorio.", nameof(symbol));

        await ConnectToMetaTrader();

        if (!_terminalRepo.IsConnected)
            throw new InvalidOperationException("MetaTrader no está conectado.");

        var normalizedSymbol = NormalizeSymbol(symbol);
        List<string>? symbolsToTrack = null;

        lock (_trackingLock)
        {
            if (_trackedSymbols.Add(normalizedSymbol))
                symbolsToTrack = _trackedSymbols.ToList();
        }

        if (symbolsToTrack == null)
            return;

        _terminalRepo.TrackPrices(symbolsToTrack);
        _logger.LogInformation("Auto-tracking enabled for symbol {Symbol}. Active tracked symbols: {Symbols}", normalizedSymbol, string.Join(", ", symbolsToTrack));
    }

    public List<string> GetSymbolList()
    {
        if (_terminalRepo == null)
            return new List<string>();

        var symbols = _terminalRepo.GetTerminal().GetSymbolList();
        // _logger.LogInformation($"Symbols retrieved: {symbols.Count} symbols.");
        return symbols.Where(x => x.TRADE_MODE != 0).Select(x => x.NAME).ToList();
    }

    private DateTime? TryParseBrokerTimestamp(string? timeValue)
    {
        if (string.IsNullOrWhiteSpace(timeValue))
            return null;

        var formats = new[]
        {
            "yyyy.MM.dd HH:mm:ss",
            "yyyy.MM.dd HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.fff"
        };

        if (DateTime.TryParseExact(timeValue.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsed))
            return parsed;

        if (DateTime.TryParse(timeValue, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsed))
            return parsed;

        return null;
    }

    public List<Rates> GetPriceHistory(string symbol)
    {
        var rates = new List<Rates>();

        try
        {
            if (_terminalRepo == null)
                return rates;

            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("El símbolo es obligatorio.", nameof(symbol));

            var timeFrameHistory = (TimeFrame)Enum.Parse(typeof(TimeFrame), "PERIOD_M1");
            var brokerQuote = _terminalRepo.GetQuote(symbol);
            var brokerTimestamp = TryParseBrokerTimestamp(brokerQuote?.TIME);
            if (!brokerTimestamp.HasValue)
            {
                _logger.LogWarning("MetaTrader no devolvió una hora válida para {Symbol}; no se solicitará histórico.", symbol);
                return rates;
            }

            DateTime endDate = brokerTimestamp.Value;
            DateTime startDate = endDate.AddHours(-12);

            rates = _terminalRepo.GetPriceHistory(symbol, timeFrameHistory, startDate, endDate);

            if (rates.Count == 0)
            {
                // Fallback with a wider range for brokers that need a longer history window.
                startDate = endDate.AddDays(-1);
                rates = _terminalRepo.GetPriceHistory(symbol, timeFrameHistory, startDate, endDate);
            }

            _logger.LogInformation($"****Price history retrieved for {symbol}: {rates.Count} records. with {startDate} {endDate}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving price history for symbol {Symbol}", symbol);
        }

        return rates;
    }

    public async Task<List<Rates>> GetDatePriceHistoryAsync(DateTime dateFilter, string symbolStr, TimeFrame timeFrameHistory, int wamPeriod)
    {
        var rateLst = new List<Rates>();

        try
        {
            // ✅ Validaciones
            if (_terminalRepo == null)
            {
                _logger.LogError("ERROR: TerminalRepo es null");
                return rateLst;
            }

            if (string.IsNullOrWhiteSpace(symbolStr))
            {
                _logger.LogError("ERROR: Symbol es requerido");
                return rateLst;
            }

            if (wamPeriod != 20 && wamPeriod != 50)
            {
                _logger.LogWarning($"WARNING: wamPeriod={wamPeriod} no es estándar (20 o 50)");
            }

            string fechaFiltro = dateFilter.Date.ToString("yyyy.MM.dd");

            // 1. ✅ Consulta asíncrona a la base de datos
            var datosExistentes = await _satiDevContext.Rates
                .Where(r =>
                    r.Fecha == fechaFiltro &&
                    r.SymbolStr == symbolStr &&
                    r.TimeFrame == timeFrameHistory.ToString()
                )
                .OrderBy(r => r.Time)
                .ToListAsync();

            if (datosExistentes != null && datosExistentes.Count > 0)
            {
                _logger.LogInformation($"✓ Datos obtenidos de DB: {datosExistentes.Count} registros para {symbolStr} en {fechaFiltro}");

                rateLst = datosExistentes.Select(dto => new Rates
                {
                    TIME = dto.Time,
                    OPEN = dto.Open,
                    HIGH = dto.High,
                    LOW = dto.Low,
                    CLOSE = dto.Close,
                    TICK_VOLUME = dto.TickVolume,
                    SPREAD = dto.Spread,
                    REAL_VOLUME = dto.RealVolume
                }).ToList();
            }
            else
            {
                _logger.LogWarning($"⚠ No hay datos en DB, consultando MetaTrader...");

                var startDate = dateFilter.Date;
                var endDate = startDate.AddDays(1);

                var result = _terminalRepo.GetPriceHistory(symbolStr, timeFrameHistory, startDate, endDate);

                if (result == null || result.Count == 0)
                {
                    _logger.LogWarning($"⚠ MetaTrader no devolvió datos para {symbolStr} en {fechaFiltro}");
                    return rateLst;
                }

                _logger.LogInformation($"✓ Datos obtenidos de MetaTrader: {result.Count} registros");

                rateLst = result.OrderBy(x => x.TIME).ToList();

                // 3. ✅ Guardar asíncronamente en la base de datos
                try
                {
                    foreach (var rate in rateLst)
                    {
                        string fecha = "";
                        string hora = "";

                        if (!string.IsNullOrEmpty(rate.TIME))
                        {
                            var parts = rate.TIME.Split(' ');
                            if (parts.Length == 2)
                            {
                                fecha = parts[0];
                                hora = parts[1];
                            }
                        }

                        await _satiDevContext.Rates.AddAsync(new Rate // ✅ AddAsync
                        {
                            Time = rate.TIME,
                            Open = rate.OPEN,
                            High = rate.HIGH,
                            Low = rate.LOW,
                            Close = rate.CLOSE,
                            TickVolume = rate.TICK_VOLUME,
                            Spread = rate.SPREAD,
                            RealVolume = rate.REAL_VOLUME,
                            SymbolStr = symbolStr,
                            TimeMtApi = rate.TIME_MTAPI,
                            Fecha = fecha,
                            Hora = hora,
                            TimeFrame = timeFrameHistory.ToString()
                        });
                    }

                    await _satiDevContext.SaveChangesAsync(); // ✅ Async
                    _logger.LogInformation($"✓ Guardados {rateLst.Count} registros en DB");
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "ERROR al guardar en DB");
                    // Continuar sin guardar, pero devolver los datos
                }
            }

            // 4. Calcular WMA
            if (rateLst.Count > 0)
            {
                int requiredPeriod = wamPeriod + 1;
                if (rateLst.Count < requiredPeriod)
                {
                    _logger.LogWarning($"⚠ Solo hay {rateLst.Count} registros, se necesitan al menos {requiredPeriod} para WAM completo");
                }

                var wmaValues = CalculateWeightedMovingAverage(rateLst, wamPeriod);
                _logger.LogInformation($"✓ WAM calculado: {wmaValues.Count} valores (período {wamPeriod})");

                // ✅ Asignar WAM y calcular porcentaje de diferencia
                for (int i = 0; i < rateLst.Count && i < wmaValues.Count; i++)
                {
                    rateLst[i].CalculatedWAM = wmaValues[i] ?? 0;

                    // ✅ Calcular porcentaje según fórmula de Excel: ABS(Close - WAM) / Close * 100
                    double closePrice = rateLst[i].CLOSE;
                    double wamValue = rateLst[i].CalculatedWAM;

                    if (closePrice != 0)
                    {
                        double percentageDiff = (Math.Abs(closePrice - wamValue) / closePrice) * 100;

                        // ✅ Redondear a 4 decimales para consistencia con Excel
                        percentageDiff = Math.Round(percentageDiff, 4);

                        // ✅ Aplicar signo: positivo si Close >= WAM, negativo si Close < WAM
                        rateLst[i].PercentageDifference = closePrice >= wamValue
                            ? percentageDiff
                            : -percentageDiff;
                    }
                    else
                    {
                        rateLst[i].PercentageDifference = 0;
                    }
                }
            }

            return rateLst;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR en GetDatePriceHistoryAsync");
            return rateLst;
        }
    }

    // ✅ Mantener versión síncrona para compatibilidad
    public List<Rates> GetDatePriceHistory(DateTime dateFilter, string symbolStr, TimeFrame timeFrameHistory, int wamPeriod)
    {
        return GetDatePriceHistoryAsync(dateFilter, symbolStr, timeFrameHistory, wamPeriod).GetAwaiter().GetResult();
    }

    public byte[] GetDatePriceHistoryExcel(DateTime dateFilter, string symbolStr, TimeFrame timeFrameHistory, int wamPeriod)
    {
        try
        {
            _logger.LogInformation($"=== EXCEL EXPORT: {symbolStr} - {dateFilter:yyyy-MM-dd} - WAM {wamPeriod} ===");

            // ✅ Obtener datos con WAM ya calculado
            var rateLst = GetDatePriceHistory(dateFilter, symbolStr, timeFrameHistory, wamPeriod);

            if (rateLst == null || rateLst.Count == 0)
            {
                _logger.LogWarning("EXCEL: No hay datos");
                return null;
            }

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Historial");

                // ✅ Encabezados - Primera fila con merge para "Compuesto"
                ws.Cells[1, 1].Value = "#";
                ws.Cells[1, 2].Value = "TIME";
                ws.Cells[1, 3].Value = "OPEN";
                ws.Cells[1, 4].Value = "HIGH";
                ws.Cells[1, 5].Value = "LOW";
                ws.Cells[1, 6].Value = "CLOSE";
                ws.Cells[1, 7].Value = "TICK_VOLUME";
                ws.Cells[1, 8].Value = "SPREAD";
                ws.Cells[1, 9].Value = "REAL_VOLUME";
                ws.Cells[1, 10].Value = "SYMBOL";
                ws.Cells[1, 11].Value = "TIME_MTAPI";

                // ✅ Merge cells para "Compuesto"
                ws.Cells[1, 12, 1, 13].Merge = true;
                ws.Cells[1, 12].Value = "Compuesto";

                // ✅ Subencabezados en fila 2
                ws.Cells[2, 12].Value = "WAM";
                ws.Cells[2, 13].Value = "%";

                // ✅ Aplicar formato a encabezados principales (fila 1)
                using (var range = ws.Cells[1, 1, 1, 11])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // ✅ Aplicar formato a "Compuesto" merge
                using (var range = ws.Cells[1, 12, 1, 13])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(74, 85, 196));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // ✅ Aplicar formato a subencabezados (fila 2)
                using (var range = ws.Cells[2, 12, 2, 13])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(74, 85, 196));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // ✅ Llenar datos desde la fila 3
                int row = 3;
                int rowNumber = 1;
                foreach (var rate in rateLst)
                {
                    ws.Cells[row, 1].Value = rowNumber;
                    ws.Cells[row, 2].Value = rate.TIME;
                    ws.Cells[row, 3].Value = rate.OPEN;
                    ws.Cells[row, 4].Value = rate.HIGH;
                    ws.Cells[row, 5].Value = rate.LOW;
                    ws.Cells[row, 6].Value = rate.CLOSE;
                    ws.Cells[row, 7].Value = rate.TICK_VOLUME;
                    ws.Cells[row, 8].Value = rate.SPREAD;
                    ws.Cells[row, 9].Value = rate.REAL_VOLUME;
                    ws.Cells[row, 10].Value = symbolStr;
                    ws.Cells[row, 11].Value = rate.TIME_MTAPI;
                    ws.Cells[row, 12].Value = rate.CalculatedWAM;
                    ws.Cells[row, 13].Value = rate.PercentageDifference;

                    // ✅ Formato de 5 decimales para WAM
                    ws.Cells[row, 12].Style.Numberformat.Format = "0.00000";

                    // ✅ Formato de porcentaje: cambiar a número con símbolo %
                    ws.Cells[row, 13].Style.Numberformat.Format = "0.0000\"%\"";

                    // ✅ Color condicional para porcentaje
                    if (rate.PercentageDifference >= 0)
                        ws.Cells[row, 13].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                    else
                        ws.Cells[row, 13].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                    row++;
                    rowNumber++;
                }

                // ✅ Aplicar formato de número a columnas de precio
                ws.Cells[3, 3, row - 1, 6].Style.Numberformat.Format = "0.00000";
                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                _logger.LogInformation($"EXCEL: Generado exitosamente con {rateLst.Count} registros");

                return package.GetAsByteArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel");
            return null;
        }
    }

    public byte[] GetDatePriceHistoryExcelWithAlgorithm(List<Rates> rates, string symbolStr, TimeFrame timeFrameHistory, DateTime dateFilter, int wamPeriod)
    {
        try
        {
            _logger.LogInformation($"=== EXCEL EXPORT WITH ALGORITHM: {symbolStr} - {dateFilter:yyyy-MM-dd} - WAM {wamPeriod} ===");

            if (rates == null || rates.Count == 0)
            {
                _logger.LogWarning("EXCEL: No hay datos");
                return null;
            }

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Historial Algoritmo");

                // Headers con 19 columnas organizadas en grupos
                // Fila 1: Grupos principales
                ws.Cells[1, 1].Value = "#";
                ws.Cells[1, 2].Value = "TIME";
                ws.Cells[1, 3].Value = "OPEN";
                ws.Cells[1, 4].Value = "HIGH";
                ws.Cells[1, 5].Value = "LOW";
                ws.Cells[1, 6].Value = "CLOSE";
                
                // Grupo Compuesto (WAM + %)
                ws.Cells[1, 7, 1, 8].Merge = true;
                ws.Cells[1, 7].Value = "Compuesto";
                
                // Grupo Algoritmo PMPn
                ws.Cells[1, 9, 1, 12].Merge = true;
                ws.Cells[1, 9].Value = "Algoritmo PMPn";
                
                // Grupo Rango Primario
                ws.Cells[1, 13, 1, 14].Merge = true;
                ws.Cells[1, 13].Value = "Rango Primario";
                
                // Grupo Tendencia
                ws.Cells[1, 15].Value = "Tendencia";
                
                // Grupo Separación
                ws.Cells[1, 16, 1, 18].Merge = true;
                ws.Cells[1, 16].Value = "Separación";
                
                // Grupo Señal
                ws.Cells[1, 19].Value = "Señal";

                // Fila 2: Subencabezados
                ws.Cells[2, 7].Value = "WAM";
                ws.Cells[2, 8].Value = "%";
                ws.Cells[2, 9].Value = "PMPn";
                ws.Cells[2, 10].Value = "max(P)";
                ws.Cells[2, 11].Value = "min(P)";
                ws.Cells[2, 12].Value = "PMPn₋₁";
                ws.Cells[2, 13].Value = "RP+";
                ws.Cells[2, 14].Value = "RP-";
                ws.Cells[2, 15].Value = "Tendencia";
                ws.Cells[2, 16].Value = "Difn";
                ws.Cells[2, 17].Value = "Prom";
                ws.Cells[2, 18].Value = "σ";
                ws.Cells[2, 19].Value = "Señal";

                // Aplicar estilos a columnas básicas (1-6)
                using (var range = ws.Cells[1, 1, 2, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Aplicar estilos a grupos con colores
                // Compuesto (#4a55c4 - azul)
                using (var range = ws.Cells[1, 7, 2, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(74, 85, 196));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Algoritmo PMPn (#667eea - violeta)
                using (var range = ws.Cells[1, 9, 2, 12])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(102, 126, 234));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Rango Primario (#f59e0b - naranja)
                using (var range = ws.Cells[1, 13, 2, 14])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(245, 158, 11));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Tendencia (#10b981 - verde)
                using (var range = ws.Cells[1, 15, 2, 15])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(16, 185, 129));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Separación (#8b5cf6 - morado)
                using (var range = ws.Cells[1, 16, 2, 18])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(139, 92, 246));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Señal (#ef4444 - rojo)
                using (var range = ws.Cells[1, 19, 2, 19])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(239, 68, 68));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // Llenar datos desde la fila 3
                int row = 3;
                int rowNumber = 1;
                foreach (var rate in rates)
                {
                    ws.Cells[row, 1].Value = rowNumber;
                    ws.Cells[row, 2].Value = FormatHistoryTime(rate.TIME);
                    ws.Cells[row, 3].Value = rate.OPEN;
                    ws.Cells[row, 4].Value = rate.HIGH;
                    ws.Cells[row, 5].Value = rate.LOW;
                    ws.Cells[row, 6].Value = rate.CLOSE;
                    ws.Cells[row, 7].Value = rate.CalculatedWAM;
                    ws.Cells[row, 8].Value = rate.PercentageDifference;
                    ws.Cells[row, 9].Value = rate.PMPn;
                    ws.Cells[row, 10].Value = rate.MaxP;
                    ws.Cells[row, 11].Value = rate.MinP;
                    ws.Cells[row, 12].Value = rate.PreviousPMPn;
                    ws.Cells[row, 13].Value = rate.RPPlus;
                    ws.Cells[row, 14].Value = rate.RPMinus;
                    ws.Cells[row, 15].Value = rate.Tendencia;
                    ws.Cells[row, 16].Value = rate.Difn;
                    ws.Cells[row, 17].Value = rate.PromDifn;
                    ws.Cells[row, 18].Value = rate.SigmaDifn;
                    ws.Cells[row, 19].Value = rate.Signal;

                    // Formatos numéricos
                    ws.Cells[row, 2].Style.Numberformat.Format = "hh:mm:ss";
                    ws.Cells[row, 3, row, 6].Style.Numberformat.Format = "0.00000"; // OHLC
                    ws.Cells[row, 7].Style.Numberformat.Format = "0.00000"; // WAM
                    ws.Cells[row, 8].Style.Numberformat.Format = "0.0000\"%\""; // %
                    ws.Cells[row, 9].Style.Numberformat.Format = "0.00000"; // PMPn
                    ws.Cells[row, 10, row, 11].Style.Numberformat.Format = "0.00000"; // MaxP, MinP
                    ws.Cells[row, 12].Style.Numberformat.Format = "0.00000"; // PreviousPMPn
                    ws.Cells[row, 13, row, 14].Style.Numberformat.Format = "0.0000\"%\""; // RP+, RP-
                    ws.Cells[row, 16, row, 18].Style.Numberformat.Format = "0.00000"; // Difn, PromDifn, SigmaDifn

                    // Colores condicionales
                    if (rate.PercentageDifference >= 0)
                        ws.Cells[row, 8].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                    else
                        ws.Cells[row, 8].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                    // Colores para Tendencia (columna 15)
                    if (rate.Tendencia == "ALZA")
                        ws.Cells[row, 15].Style.Font.Color.SetColor(System.Drawing.Color.Green);
                    else if (rate.Tendencia == "BAJA")
                        ws.Cells[row, 15].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    else if (rate.Tendencia == "NEUTRO")
                        ws.Cells[row, 15].Style.Font.Color.SetColor(System.Drawing.Color.Gray);

                    // Colores para Señal (columna 19)
                    if (rate.Signal == "COMPRA")
                    {
                        ws.Cells[row, 19].Style.Font.Color.SetColor(System.Drawing.Color.White);
                        ws.Cells[row, 19].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 19].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);
                        ws.Cells[row, 19].Style.Font.Bold = true;
                    }
                    else if (rate.Signal == "VENTA")
                    {
                        ws.Cells[row, 19].Style.Font.Color.SetColor(System.Drawing.Color.White);
                        ws.Cells[row, 19].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, 19].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                        ws.Cells[row, 19].Style.Font.Bold = true;
                    }
                    else if (rate.Signal == "NINGUNA")
                    {
                        ws.Cells[row, 19].Style.Font.Color.SetColor(System.Drawing.Color.Gray);
                    }

                    row++;
                    rowNumber++;
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Column(3).Hidden = true; // OPEN
                ws.Column(4).Hidden = true; // HIGH
                ws.Column(5).Hidden = true; // LOW
                _logger.LogInformation($"EXCEL WITH ALGORITHM: Generado con {rates.Count} registros y 19 columnas");

                return package.GetAsByteArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel with algorithm");
            return null;
        }
    }

    public void DisconnectFromMetaTrader()
    {
        // Logic to disconnect from MetaTrader
    }

    private static string NormalizeSymbol(string symbol) => symbol.Trim().ToUpperInvariant();

    private static string FormatHistoryTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var separatorIndex = value.IndexOf(' ');
        if (separatorIndex >= 0 && separatorIndex + 1 < value.Length)
            return value[(separatorIndex + 1)..];

        var isoSeparatorIndex = value.IndexOf('T');
        if (isoSeparatorIndex >= 0 && isoSeparatorIndex + 1 < value.Length)
            return value[(isoSeparatorIndex + 1)..];

        return value;
    }

    public void Mt5_OnPrice(object? sender, Quote e)
    {
        if (e == null || string.IsNullOrWhiteSpace(e.SYMBOL))
            return;

        var groupName = MetaTraderHub.BuildSymbolGroup(e.SYMBOL);
        _ = _mtHubContext.Clients
            .Group(groupName)
            .SendAsync("ReceiveMetaTraderData", e);
    }

    // Por qué redondear en el backend:
    // ✅ Consistencia: Todos los clientes (Web, Excel, Mobile) reciben los mismos valores
    // ✅ Performance: El servidor hace el cálculo una vez, no cada cliente
    // ✅ Precisión: Evitas problemas de redondeo de JavaScript
    // ✅ Menos tráfico: Envías menos bytes por la red
    // ✅ Lógica de negocio: El cálculo financiero debe estar centralizado
    /// <summary>
    /// Calcula el Weighted Moving Average (WMA) - Promedio Móvil Ponderado
    /// Basado en la fórmula de Excel: SUMPRODUCT(Precios, Multiplicadores) / SUM(Multiplicadores)
    /// </summary>
    private List<double?> CalculateWeightedMovingAverage(List<Rates> rates, int period)
    {
        var wmaValues = new List<double?>();

        if (rates == null || rates.Count == 0)
            return wmaValues;

        // Excel usa period + 1 multiplicadores (21 para período 20, 51 para período 50)
        int actualPeriod = period + 1;

        _logger.LogInformation($"=== Iniciando cálculo WAM período={period} (actualPeriod={actualPeriod}) ===");

        for (int i = 0; i < rates.Count; i++)
        {
            // Determinar cuántos valores usar
            int valuesAvailable = i + 1;
            int valuesForWam = Math.Min(valuesAvailable, actualPeriod);

            double sumProduct = 0;
            int sumOfMultipliers = 0;

            // ✅ CORRECCIÓN CRÍTICA: Usar ventana móvil para TODOS los loops
            for (int j = 0; j < valuesForWam; j++)
            {
                int multiplier = j + 1;

                // ✅ CLAVE: Calcular índice correcto para ventana móvil
                // Loop 1 (i=0): priceIndex = 0 - 0 + 0 = 0 (usa rates[0])
                // Loop 2 (i=1): j=0 -> priceIndex = 1 - 1 + 0 = 0, j=1 -> priceIndex = 1 - 1 + 1 = 1 (usa rates[0,1])
                // Loop 21 (i=20): usa rates[0..20]
                // Loop 22 (i=21): usa rates[1..21] (ventana móvil)
                int priceIndex = i - (valuesForWam - 1) + j;

                double closePrice = rates[priceIndex].CLOSE;

                sumProduct += multiplier * closePrice;
                sumOfMultipliers += multiplier;

                // Debug para las primeras 3 filas y filas 20-22
                if (i < 3 || (i >= 19 && i <= 22))
                {
                    _logger.LogInformation(
                        $"  i={i + 1}, j={j}: mult={multiplier}, priceIdx={priceIndex}, " +
                        $"close={closePrice:F5}, sumProd={sumProduct:F5}");
                }
            }

            double wma = sumProduct / sumOfMultipliers;
            double roundedWma = Math.Round(wma, 5);
            wmaValues.Add(roundedWma);

            // Log resumen
            if (i < 3 || (i >= 19 && i <= 22))
            {
                _logger.LogInformation(
                    $"WAM Row {i + 1}: valuesUsed={valuesForWam}, sumProd={sumProduct:F10}, " +
                    $"sumMult={sumOfMultipliers}, WAM={roundedWma:F5}, Close={rates[i].CLOSE:F5}");
            }
        }

        _logger.LogInformation($"=== WAM calculado: {wmaValues.Count} valores ===");

        return wmaValues;
    }
}
