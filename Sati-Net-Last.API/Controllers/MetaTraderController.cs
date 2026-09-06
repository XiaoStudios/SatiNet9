using Microsoft.AspNetCore.Mvc;
using Sati_Models.Dtos;
using Sati_Net_Last.API.MTRepositories.Interfaces;
using Sati_Net_Last.API.Repositories.Interfaces;
using Sati_Net_Last.API.Services;

namespace Sati_Net_Last.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetaTraderController : ControllerBase
{
    private readonly ILogger<MetaTraderController> _logger;
    private readonly OperativeAlgorithmSvc _operativeAlgorithmSvc;
    private readonly IMTRepo _metaTrader;
    private readonly IAuthRepository _authRepository;

    public MetaTraderController
    (
        ILogger<MetaTraderController> logger,
        OperativeAlgorithmSvc operativeAlgorithmSvc,
        IMTRepo metaTrader,
        IAuthRepository authRepository
    )
    {
        _logger = logger;
        _metaTrader = metaTrader;
        _operativeAlgorithmSvc = operativeAlgorithmSvc;
        _authRepository = authRepository;
    }

    [HttpPost("ConnectToMT")]
    public async Task<IActionResult> ConnectToMT()
    {
        try
        {
            await _metaTrader.ConnectToMetaTrader();
            return Ok("Connected to MetaTrader");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MetaTrader.");
            return StatusCode(500, "Internal server error while connecting to MetaTrader.");
        }
    }

    [HttpPost("TrackSymbols")]
    public async Task<IActionResult> TrackSymbols([FromBody] List<string> symbols)
    {
        try
        {
            await _metaTrader.TrackSymbolsAsync(symbols ?? new List<string>());
            return Ok(new
            {
                success = true,
                symbols = symbols ?? new List<string>()
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating symbols tracked by MetaTrader.");
            return StatusCode(500, "Error updating MetaTrader tracking.");
        }
    }

    [HttpPost("TrackSymbolsForUser")]
    public async Task<IActionResult> TrackSymbolsForUser([FromQuery] int userId)
    {
        if (userId <= 0)
            return BadRequest(new { success = false, message = "El usuario es obligatorio." });

        try
        {
            var symbols = await _authRepository.GetActiveSymbolsByUserIdAsync(userId);

            if (symbols == null || symbols.Count == 0)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = "Tu usuario no tiene símbolos asignados. Habla con el administrador para que te asigne uno."
                });
            }

            await _metaTrader.TrackSymbolsAsync(symbols);

            return Ok(new
            {
                success = true,
                symbols,
                selectedSymbol = symbols.First()
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking symbols for user {UserId}", userId);
            return StatusCode(500, new { success = false, message = "Error actualizando tracking para el usuario." });
        }
    }

    [HttpGet("GetSymbolList")]
    public IActionResult GetSymbolList()
    {
        try
        {
            var symbols = _metaTrader.GetSymbolList();
            return Ok(symbols);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving symbol list.");
            return StatusCode(500, "Internal server error while retrieving symbol list.");
        }
    }

    [HttpPost("GetPriceHistory")]
    [HttpGet("GetPriceHistory")]
    public IActionResult GetPriceHistory([FromQuery] string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest("El símbolo es obligatorio.");

        try
        {
            var priceHistory = _metaTrader.GetPriceHistory(symbol);
            return Ok(priceHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving price history for symbol {Symbol}.", symbol);
            return StatusCode(500, "Internal server error while retrieving price history.");
        }
    }

    [HttpGet("CheckRealtimeAccess")]
    public async Task<IActionResult> CheckRealtimeAccess([FromQuery] int userId)
    {
        if (userId <= 0)
            return BadRequest("El usuario es obligatorio.");

        var symbols = await _authRepository.GetActiveSymbolsByUserIdAsync(userId);

        if (symbols == null || symbols.Count == 0)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                success = false,
                message = "Tu usuario no tiene símbolos asignados. Habla con el administrador para que te asigne uno."
            });
        }

        return Ok(new
        {
            success = true,
            symbols,
            selectedSymbol = symbols.First()
        });
    }

    // [HttpGet("GetDatePriceHistory")]
    // public IActionResult GetDatePriceHistory(DateTime dateFilter, string symbolStr)
    // {
    //     try
    //     {
    //         var priceHistory = _metaTrader.GetDatePriceHistory(dateFilter, symbolStr);
    //         return Ok(priceHistory);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error retrieving date price history.");
    //         return StatusCode(500, "Internal server error while retrieving date price history.");
    //     }
    // }

    [HttpGet("GetDatePriceHistory")]
    public async Task<IActionResult> GetDatePriceHistory(DateTime dateFilter, string symbolStr, string timeFrame, int wamPeriod) // ✅ async
    {
        if (!HistoryTimeFrameParser.TryParse(timeFrame, out var parsedTimeFrame))
            return BadRequest("El periodo especificado no es válido. Debe ser M1, M2, M3, M4, M5 o M6.");
        
        try
        {
            var rates = await _metaTrader.GetDatePriceHistoryAsync(dateFilter, symbolStr, parsedTimeFrame, wamPeriod); // ✅ await
            return Ok(rates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving price history.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("GetDatePriceHistoryExcel")]
    public IActionResult GetDatePriceHistoryExcel(DateTime dateFilter, string symbolStr, string timeFrame, int wamPeriod)
    {
        if (!HistoryTimeFrameParser.TryParse(timeFrame, out var parsedTimeFrame))
            return BadRequest("El periodo especificado no es válido. Debe ser M1, M2, M3, M4, M5 o M6.");

        try
        {
            // var excelBytes = (_metaTrader as Sati_Net_Last.API.MTRepositories.MTRepo)?.GetDatePriceHistoryExcel(dateFilter, symbolStr);
            var excelBytes = _metaTrader.GetDatePriceHistoryExcel(dateFilter, symbolStr, parsedTimeFrame, wamPeriod);

            if (excelBytes == null)
                return NotFound("No data found for the specified date and symbol.");

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Historial_{symbolStr}_{dateFilter:yyyyMMdd}.xlsx"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel file.");
            return StatusCode(500, "Internal server error while generating Excel file.");
        }
    }

    [HttpGet("GetDatePriceHistoryWithAlgorithm")]
    public async Task<IActionResult> GetDatePriceHistoryWithAlgorithm
    (
        DateTime dateFilter,
        string symbolStr,
        string timeFrame,
        int wamPeriod,
        [FromQuery] double? pt = null,
        [FromQuery] double? pr = null,
        [FromQuery] double? sigma = null,
        [FromQuery] double? mp = null,
        [FromQuery] double? fd = null
    )
    {
        if (!HistoryTimeFrameParser.TryParse(timeFrame, out var parsedTimeFrame))
            return BadRequest("El periodo especificado no es válido. Debe ser M1, M2, M3, M4, M5 o M6.");

        try
        {
            _logger.LogInformation($"GetDatePriceHistoryWithAlgorithm: {symbolStr}, {dateFilter:yyyy-MM-dd}, WAM {wamPeriod}");

            // 1. Obtener datos básicos con WAM (método existente)
            var rates = await _metaTrader.GetDatePriceHistoryAsync(dateFilter, symbolStr, parsedTimeFrame, wamPeriod);

            if (rates == null || rates.Count == 0)
            {
                return NotFound("No hay datos disponibles");
            }

            // 2. Configurar parámetros del algoritmo
            var parameters = new ParametersAlgorithmDto
            {
                Period = wamPeriod,
                Pt = pt ?? 0.05,
                Pr = pr ?? 0.75,
                Sigma = sigma ?? 2.0,
                Mp = mp ?? 1000,
                Fd = fd ?? 0.10
            };

            if (!parameters.IsValid())
            {
                return BadRequest("Parámetros del algoritmo inválidos");
            }

            // 3. Calcular algoritmo operativo (BATCH)
            rates = _operativeAlgorithmSvc.CalculateBatch(rates, wamPeriod, parameters);

            _logger.LogInformation($"Algoritmo calculado. Señales: {rates.Count(r => r.Signal != "NINGUNA")}");

            return Ok(rates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetDatePriceHistoryWithAlgorithm");
            return StatusCode(500, "Error al calcular el algoritmo");
        }
    }

    [HttpGet("GetDatePriceHistoryExcelWithAlgorithm")]
    public async Task<IActionResult> GetDatePriceHistoryExcelWithAlgorithm
    (
        DateTime dateFilter,
        string symbolStr,
        string timeFrame,
        int wamPeriod,
        [FromQuery] double? pt = null,
        [FromQuery] double? pr = null,
        [FromQuery] double? sigma = null,
        [FromQuery] double? mp = null,
        [FromQuery] double? fd = null
    )
    {
        if (!HistoryTimeFrameParser.TryParse(timeFrame, out var parsedTimeFrame))
            return BadRequest("El periodo especificado no es válido. Debe ser M1, M2, M3, M4, M5 o M6.");
        
        try
        {
            _logger.LogInformation($"GetDatePriceHistoryExcelWithAlgorithm: {symbolStr}, {dateFilter:yyyy-MM-dd}, WAM {wamPeriod}");

            // 1. Obtener datos básicos con WAM
            var rates = await _metaTrader.GetDatePriceHistoryAsync(dateFilter, symbolStr, parsedTimeFrame, wamPeriod);

            if (rates == null || rates.Count == 0)
            {
                return NotFound("No hay datos disponibles");
            }

            // 2. Configurar parámetros del algoritmo
            var parameters = new ParametersAlgorithmDto
            {
                Period = wamPeriod,
                Pt = pt ?? 0.05,
                Pr = pr ?? 0.75,
                Sigma = sigma ?? 2.0,
                Mp = mp ?? 1000,
                Fd = fd ?? 0.10
            };

            if (!parameters.IsValid())
            {
                return BadRequest("Parámetros del algoritmo inválidos");
            }

            // 3. Calcular algoritmo operativo (BATCH)
            rates = _operativeAlgorithmSvc.CalculateBatch(rates, wamPeriod, parameters);

            _logger.LogInformation($"Algoritmo calculado. Generando Excel con 19 columnas...");

            // 4. Generar Excel con todas las columnas del algoritmo
            var excelBytes = _metaTrader.GetDatePriceHistoryExcelWithAlgorithm(rates, symbolStr, parsedTimeFrame, dateFilter, wamPeriod);

            if (excelBytes == null)
            {
                return NotFound("Error al generar el archivo Excel");
            }

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Historial_{symbolStr}_{dateFilter:yyyyMMdd}_Algoritmo.xlsx"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel with algorithm.");
            return StatusCode(500, "Internal server error while generating Excel file with algorithm.");
        }
    }

}
