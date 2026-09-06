using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Sati_Net_Last.Web.Controllers;

public class HistoryMTController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HistoryMTController> _logger;

    public HistoryMTController(IHttpClientFactory httpClientFactory, ILogger<HistoryMTController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var symbols = new List<string>();

        try
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(userId))
                return RedirectToAction("Login", "Account");

            var userSymbolsJson = HttpContext.Session.GetString("UserSymbols");
            if (!string.IsNullOrWhiteSpace(userSymbolsJson))
            {
                symbols = JsonSerializer.Deserialize<List<string>>(userSymbolsJson) ?? new List<string>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar los símbolos asignados al usuario");
        }

        ViewBag.Symbols = symbols;
        ViewBag.NoSymbolsMessage = symbols.Count == 0
            ? "No tienes símbolos asignados. Habla con el administrador para que te asigne uno para consultar historial."
            : null;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> GetDatePriceHistory(DateTime dateFilter, string symbolStr, string timeFrame, int wamPeriod)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendAPI");
            var response = await client.GetAsync($"api/MetaTrader/GetDatePriceHistory?dateFilter={dateFilter:yyyy-MM-dd}&symbolStr={symbolStr}&timeFrame={timeFrame}&wamPeriod={wamPeriod}");

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
            _logger.LogError(ex, "Error al obtener el historial de precios por fecha");
            return StatusCode(500, "Error al cargar el historial de precios desde el API");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDatePriceHistoryExcel(DateTime dateFilter, string symbolStr, string timeFrame, int wamPeriod)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendAPI");
            var response = await client.GetAsync($"api/MetaTrader/GetDatePriceHistoryExcel?dateFilter={dateFilter:yyyy-MM-dd}&symbolStr={symbolStr}&timeFrame={timeFrame}&wamPeriod={wamPeriod}");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Historial_{symbolStr}_{dateFilter:yyyyMMdd}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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
            _logger.LogError(ex, "Error al obtener el historial de precios por fecha");
            return StatusCode(500, "Error al cargar el historial de precios desde el API");
        }
    }

    [HttpPost]
    public async Task<IActionResult> GetDatePriceHistoryWithAlgorithm
    (
        DateTime dateFilter,
        string symbolStr,
        string timeFrame,
        int wamPeriod,
        double? pt = null,
        double? pr = null,
        double? sigma = null,
        double? mp = null,
        double? fd = null
    )
    {
        try
        {
            var client = _httpClientFactory.CreateClient("BackendAPI");

            // Construir query string con parámetros opcionales
            var queryParams = new List<string>
            {
                $"dateFilter={dateFilter:yyyy-MM-dd}",
                $"symbolStr={symbolStr}",
                $"timeFrame={timeFrame}",
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

    [HttpGet]
    public async Task<IActionResult> GetDatePriceHistoryExcelWithAlgorithm(
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
            var response = await client.GetAsync($"api/MetaTrader/GetDatePriceHistoryExcelWithAlgorithm?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Historial_{symbolStr}_{dateFilter:yyyyMMdd}_Algoritmo.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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
            _logger.LogError(ex, "Error al obtener el Excel con algoritmo");
            return StatusCode(500, "Error al generar el archivo Excel con algoritmo");
        }
    }
}