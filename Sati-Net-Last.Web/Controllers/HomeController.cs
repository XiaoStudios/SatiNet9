using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Sati_Net_Last.Web.Models;

namespace Sati_Net_Last.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrWhiteSpace(userId))
            return RedirectToAction("Login", "Account");

        var userSymbols = HttpContext.Session.GetString("UserSymbols");
        var symbols = !string.IsNullOrWhiteSpace(userSymbols)
            ? JsonSerializer.Deserialize<List<string>>(userSymbols) ?? new List<string>()
            : new List<string>();

        var selectedSymbol = HttpContext.Session.GetString("SelectedSymbol");
        if (string.IsNullOrWhiteSpace(selectedSymbol) && symbols.Count > 0)
        {
            selectedSymbol = symbols[0];
            HttpContext.Session.SetString("SelectedSymbol", selectedSymbol);
        }

        ViewBag.UserSymbols = symbols;
        ViewBag.SelectedSymbol = selectedSymbol;
        ViewBag.HasSymbols = symbols.Count > 0;
        return View();
    }

    [HttpPost]
    public IActionResult SetSelectedSymbol(string symbol)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var userSymbols = HttpContext.Session.GetString("UserSymbols");
        var authorizedSymbols = !string.IsNullOrWhiteSpace(userSymbols)
            ? JsonSerializer.Deserialize<List<string>>(userSymbols) ?? new List<string>()
            : new List<string>();

        if (string.IsNullOrWhiteSpace(symbol) || authorizedSymbols.Count == 0 || !authorizedSymbols.Contains(symbol, StringComparer.OrdinalIgnoreCase))
            return Forbid();

        HttpContext.Session.SetString("SelectedSymbol", symbol.Trim());
        return Ok(new { success = true, symbol = symbol.Trim() });
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
