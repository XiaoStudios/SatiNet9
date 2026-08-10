using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sati_Net_Last.Admin.Models;

namespace Sati_Net_Last.Admin.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ToggleMTConnection()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("http://localhost:5289/api/MetaTrader/ConnectToMT", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Conexión a MetaTrader establecida correctamente.";
            }
            else
            {
                TempData["Mensaje"] = $"Error al conectar a MetaTrader: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            TempData["Mensaje"] = $"Error: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
