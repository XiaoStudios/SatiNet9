using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sati_Net_Last.Admin.Data;
using Sati_Net_Last.Admin.Models;

namespace Sati_Net_Last.Admin.Controllers;

[Authorize]
public class UsuariosController : Controller
{
    private readonly AdminDbContext _db;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(AdminDbContext db, ILogger<UsuariosController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Pares de monedas disponibles
    private static readonly List<string> ParesMonedas = new()
    {
        "EURMXN",
        "USDJPY",
        "GBPUSD",
        "EURJPY"
    };

    // GET: Usuarios
    public async Task<IActionResult> Index()
    {
        var usuarios = await _db.UsuariosBroker
            .OrderByDescending(u => u.FechaRegistro)
            .ToListAsync();
        return View(usuarios);
    }

    // GET: Usuarios/Create
    public IActionResult Create()
    {
        ViewBag.ParesMonedas = ParesMonedas;
        return View();
    }

    // POST: Usuarios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NombreCompleto,Correo,ParMoneda,Activo")] UsuarioBroker usuario)
    {
        if (ModelState.IsValid)
        {
            usuario.FechaRegistro = DateTime.UtcNow;
            _db.UsuariosBroker.Add(usuario);
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Cliente creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.ParesMonedas = ParesMonedas;
        return View(usuario);
    }
}
