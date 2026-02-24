using Microsoft.AspNetCore.Mvc;
using Sati_Net_Last.API.MTRepositories.Interfaces;

namespace Sati_Net_Last.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetaTraderController : ControllerBase
{
    private readonly ILogger<MetaTraderController> _logger;
    private readonly IMTRepo _metaTrader;

    public MetaTraderController
    (
        ILogger<MetaTraderController> logger,
        IMTRepo metaTrader
    )
    {
        _logger = logger;
        _metaTrader = metaTrader;
    }

    [HttpPost("ConnectToMT")]
    public IActionResult ConnectToMT()
    {
        try
        {
            _metaTrader.ConnectToMetaTrader();
            return Ok("Connected to MetaTrader");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MetaTrader.");
            return StatusCode(500, "Internal server error while connecting to MetaTrader.");
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
    public IActionResult GetPriceHistory()
    {
        try
        {
            var priceHistory = _metaTrader.GetPriceHistory();
            return Ok(priceHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving price history.");
            return StatusCode(500, "Internal server error while retrieving price history.");
        }
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
    public async Task<IActionResult> GetDatePriceHistory(DateTime dateFilter, string symbolStr, int wamPeriod) // ✅ async
    {
        try
        {
            var rates = await _metaTrader.GetDatePriceHistoryAsync(dateFilter, symbolStr, wamPeriod); // ✅ await
            return Ok(rates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving price history.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("GetDatePriceHistoryExcel")]
    public IActionResult GetDatePriceHistoryExcel(DateTime dateFilter, string symbolStr, int wamPeriod)
    {
        try
        {
            // var excelBytes = (_metaTrader as Sati_Net_Last.API.MTRepositories.MTRepo)?.GetDatePriceHistoryExcel(dateFilter, symbolStr);
            var excelBytes = _metaTrader.GetDatePriceHistoryExcel(dateFilter, symbolStr, wamPeriod);

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
}
