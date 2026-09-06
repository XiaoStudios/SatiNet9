using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sati_Models.DTOs;

namespace Sati_Net_Last.Admin.Controllers;

[Authorize]
public class MtapiSettingsController : Controller
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<MtapiSettingsController> _logger;

    public MtapiSettingsController(IHttpClientFactory httpFactory, ILogger<MtapiSettingsController> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    // GET: MtapiSettings
    public async Task<IActionResult> Index()
    {
        var client = _httpFactory.CreateClient("BackendAPI");
        var resp = await client.GetAsync("api/MtapiSettings");
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Fallo al obtener configuraciones MT from API: {Status}", resp.StatusCode);
            TempData["Mensaje"] = "No fue posible obtener las configuraciones desde la API.";
            return View(Enumerable.Empty<MtapiSettingDto>());
        }

        var list = await resp.Content.ReadFromJsonAsync<List<MtapiSettingDto>>() ?? new List<MtapiSettingDto>();
        var ordered = list.OrderByDescending(m => m.UpdatedAt);
        return View(ordered);
    }

    // GET: MtapiSettings/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: MtapiSettings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MtapiSettingDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        dto.LastConnectionStatus ??= "No conectado";

        var client = _httpFactory.CreateClient("BackendAPI");
        var resp = await client.PostAsJsonAsync("api/MtapiSettings", dto);
        if (resp.IsSuccessStatusCode)
        {
            TempData["Mensaje"] = "Configuración de MetaTrader creada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Error al crear la configuración en la API.");
        return View(dto);
    }

    // GET: MtapiSettings/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var client = _httpFactory.CreateClient("BackendAPI");
        var resp = await client.GetAsync($"api/MtapiSettings/{id}");
        if (!resp.IsSuccessStatusCode)
            return NotFound();

        var dto = await resp.Content.ReadFromJsonAsync<MtapiSettingDto>();
        if (dto == null) return NotFound();
        return View(dto);
    }

    // POST: MtapiSettings/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MtapiSettingDto dto)
    {
        if (id != dto.Id) return NotFound();
        if (!ModelState.IsValid) return View(dto);

        dto.UpdatedAt = DateTime.UtcNow;
        var client = _httpFactory.CreateClient("BackendAPI");
        var resp = await client.PutAsJsonAsync($"api/MtapiSettings/{id}", dto);
        if (resp.IsSuccessStatusCode)
        {
            TempData["Mensaje"] = "Configuración de MetaTrader actualizada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Error al actualizar la configuración en la API.");
        return View(dto);
    }

    // POST: MtapiSettings/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var client = _httpFactory.CreateClient("BackendAPI");
        var resp = await client.DeleteAsync($"api/MtapiSettings/{id}");
        if (resp.IsSuccessStatusCode)
        {
            TempData["Mensaje"] = "Configuración de MetaTrader eliminada exitosamente.";
        }
        else
        {
            TempData["Mensaje"] = "No fue posible eliminar la configuración en la API.";
        }

        return RedirectToAction(nameof(Index));
    }
    // --- ENDPOINTS PUENTE HACIA LA API DE IVÁN ---

    [HttpPost]
    public async Task<IActionResult> TestConnection()
    {
        var client = _httpFactory.CreateClient("BackendAPI");
        // NOTA: Ajusta la ruta "api/MetaTrader/test" al endpoint exacto que Iván haya programado en la API principal
        var resp = await client.PostAsync("api/MetaTrader/test", null);
        return resp.IsSuccessStatusCode ? Ok() : StatusCode((int)resp.StatusCode);
    }

    [HttpPost]
    public async Task<IActionResult> Connect()
    {
        var client = _httpFactory.CreateClient("BackendAPI");
        // NOTA: Ajusta la ruta "api/MetaTrader/connect" al endpoint exacto de Iván
        var resp = await client.PostAsync("api/MetaTrader/connect", null);
        return resp.IsSuccessStatusCode ? Ok() : StatusCode((int)resp.StatusCode);
    }

    [HttpPost]
    public async Task<IActionResult> Disconnect()
    {
        var client = _httpFactory.CreateClient("BackendAPI");
        // NOTA: Ajusta la ruta "api/MetaTrader/disconnect" al endpoint exacto de Iván
        var resp = await client.PostAsync("api/MetaTrader/disconnect", null);
        return resp.IsSuccessStatusCode ? Ok() : StatusCode((int)resp.StatusCode);
    }
}
