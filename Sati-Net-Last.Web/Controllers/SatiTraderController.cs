using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Sati_Net_Last.Web.Controllers;

public class SatiTraderController : Controller
{
    private readonly IConfiguration _configuration;

    public SatiTraderController(IConfiguration configuration)
    {
        _configuration = configuration;
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
        ViewBag.NoSymbolsMessage = symbols.Count == 0
            ? "Tu usuario no tiene símbolos asignados. Habla con el administrador para que te asigne uno."
            : null;
        ViewBag.BackendApiBaseUrl = _configuration["BackendAPI:BaseUrl"] ?? "http://localhost:5289/";

        return View();
    }
}