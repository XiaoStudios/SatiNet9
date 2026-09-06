using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sati_Models.DTOs;

namespace Sati_Net_Last.Admin.Controllers;

[Authorize]
public class UsuariosController : Controller
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(IHttpClientFactory httpFactory, ILogger<UsuariosController> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    // Símbolos disponibles
    private static readonly List<string> Simbolos = new()
    {
        "EURMXN",
        "USDJPY",
        "GBPUSD",
        "EURJPY"
    };

    // GET: Usuarios (Lista de asignaciones de símbolos)
    public async Task<IActionResult> Index()
    {
        var client = _httpFactory.CreateClient("BackendAPI");
        var resp = await client.GetAsync("api/UserSymbols");
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("No se pudieron obtener los símbolos desde la API: {Status}", resp.StatusCode);
            return View(Enumerable.Empty<UserSymbolDto>());
        }

        var list = await resp.Content.ReadFromJsonAsync<List<UserSymbolDto>>() ?? new List<UserSymbolDto>();
        var ordered = list.OrderByDescending(u => u.AssignedAt);
        return View(ordered);
    }

    // GET: Usuarios/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Simbolos = Simbolos;
        // Obtener lista de admins desde la API
        var client = _httpFactory.CreateClient("BackendAPI");
        var resp = await client.GetAsync("api/Admins");
        var admins = new List<dynamic>();
        if (resp.IsSuccessStatusCode)
        {
            admins = await resp.Content.ReadFromJsonAsync<List<dynamic>>() ?? new List<dynamic>();
        }

        ViewBag.Usuarios = new SelectList(admins.Select(a => new { Id = (int)a.Id, FullName = (string)a.FullName }), "Id", "FullName");
        return View();
    }

    // POST: Usuarios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserSymbolDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Simbolos = Simbolos;
            return View(dto);
        }

        dto.AssignedAt = DateTime.UtcNow;
        var client = _httpFactory.CreateClient("BackendAPI");
        var resp = await client.PostAsJsonAsync("api/UserSymbols", dto);
        if (resp.IsSuccessStatusCode)
        {
            TempData["Mensaje"] = "Símbolo asignado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Error al asignar símbolo en la API.");
        ViewBag.Simbolos = Simbolos;
        return View(dto);
    }

    // GET: Usuarios/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var client = _httpFactory.CreateClient("BackendAPI");
        var resp = await client.GetAsync($"api/UserSymbols/{id}");
        if (!resp.IsSuccessStatusCode) return NotFound();

        var dto = await resp.Content.ReadFromJsonAsync<UserSymbolDto>();
        if (dto == null) return NotFound();

        ViewBag.Simbolos = Simbolos;
        var adminsResp = await client.GetAsync("api/Admins");
        var admins = new List<dynamic>();
        if (adminsResp.IsSuccessStatusCode)
            admins = await adminsResp.Content.ReadFromJsonAsync<List<dynamic>>() ?? new List<dynamic>();

        ViewBag.Usuarios = new SelectList(admins.Select(a => new { Id = (int)a.Id, FullName = (string)a.FullName }), "Id", "FullName", dto.UserId);
        return View(dto);
    }

    // POST: Usuarios/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserSymbolDto dto)
    {
        if (id != dto.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Simbolos = Simbolos;
            return View(dto);
        }

        var client = _httpFactory.CreateClient("BackendAPI");
        var resp = await client.PutAsJsonAsync($"api/UserSymbols/{id}", dto);
        if (resp.IsSuccessStatusCode)
        {
            TempData["Mensaje"] = "Símbolo actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Error al actualizar símbolo en la API.");
        ViewBag.Simbolos = Simbolos;
        return View(dto);
    }

    // POST: Usuarios/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var client = _httpFactory.CreateClient("BackendAPI");
        var resp = await client.DeleteAsync($"api/UserSymbols/{id}");
        if (resp.IsSuccessStatusCode)
        {
            TempData["Mensaje"] = "Símbolo eliminado exitosamente.";
        }
        else
        {
            TempData["Mensaje"] = "No fue posible eliminar el símbolo en la API.";
        }

        return RedirectToAction(nameof(Index));
    }
    // --- PUENTE AJAX PARA LA PESTAÑA DEL PANEL DE ADMINISTRACIÓN ---

    [HttpPost]
    public async Task<IActionResult> CrearAsignacionAjax(string email, string simbolos)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(simbolos))
            return BadRequest("El correo y los símbolos son obligatorios.");

        var client = _httpFactory.CreateClient("BackendAPI");

        // 1. Consultar a la API para obtener la lista de usuarios (Admins)
        var respAdmins = await client.GetAsync("api/Admins");
        if (!respAdmins.IsSuccessStatusCode)
            return StatusCode(500, "Error al consultar la lista de usuarios en la API.");

        var admins = await respAdmins.Content.ReadFromJsonAsync<List<AdminUserTempDto>>() ?? new List<AdminUserTempDto>();

        // Buscar al usuario por correo
        var usuario = admins.FirstOrDefault(a => a.Email != null && a.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
        if (usuario == null)
            return NotFound($"No se encontró al usuario con correo {email} en la base de datos.");

        // 2. Limpiar y separar los símbolos (ej. "EURUSD, GBPUSD" -> ["EURUSD", "GBPUSD"])
        var listaSimbolos = simbolos.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // 3. Enviar cada símbolo a la API de Iván para guardarlo
        foreach (var sim in listaSimbolos)
        {
            var dto = new UserSymbolDto
            {
                UserId = usuario.Id,
                SymbolStr = sim.ToUpper(),
                IsActive = true,
                AssignedAt = DateTime.UtcNow
            };
            await client.PostAsJsonAsync("api/UserSymbols", dto);
        }

        return Ok();
    }

    // Clase auxiliar para mapear la respuesta de la API de Iván
    public class AdminUserTempDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
    }
}

