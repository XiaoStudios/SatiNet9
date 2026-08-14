using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        var userSymbols = await _db.UserSymbols
            .Include(u => u.User)
            .OrderByDescending(u => u.AssignedAt)
            .ToListAsync();
        return View(userSymbols);
    }

    // GET: Usuarios/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Simbolos = Simbolos;
        ViewBag.Usuarios = new SelectList(await _db.Admins.ToListAsync(), "Id", "FullName");
        return View();
    }

    // POST: Usuarios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserId,SymbolStr,IsActive")] UserSymbol userSymbol)
    {
        if (ModelState.IsValid)
        {
            userSymbol.AssignedAt = DateTime.UtcNow;
            _db.UserSymbols.Add(userSymbol);
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Símbolo asignado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Simbolos = Simbolos;
        ViewBag.Usuarios = new SelectList(await _db.Admins.ToListAsync(), "Id", "FullName", userSymbol.UserId);
        return View(userSymbol);
    }

    // GET: Usuarios/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var userSymbol = await _db.UserSymbols.FindAsync(id);
        if (userSymbol == null)
            return NotFound();

        ViewBag.Simbolos = Simbolos;
        ViewBag.Usuarios = new SelectList(await _db.Admins.ToListAsync(), "Id", "FullName", userSymbol.UserId);
        return View(userSymbol);
    }

    // POST: Usuarios/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,SymbolStr,IsActive")] UserSymbol userSymbol)
    {
        if (id != userSymbol.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _db.UserSymbols.Update(userSymbol);
                await _db.SaveChangesAsync();
                TempData["Mensaje"] = "Símbolo actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserSymbolExists(userSymbol.Id))
                    return NotFound();
                throw;
            }
        }

        ViewBag.Simbolos = Simbolos;
        ViewBag.Usuarios = new SelectList(await _db.Admins.ToListAsync(), "Id", "FullName", userSymbol.UserId);
        return View(userSymbol);
    }

    // POST: Usuarios/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userSymbol = await _db.UserSymbols.FindAsync(id);
        if (userSymbol != null)
        {
            _db.UserSymbols.Remove(userSymbol);
            await _db.SaveChangesAsync();
            TempData["Mensaje"] = "Símbolo eliminado exitosamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool UserSymbolExists(int id)
    {
        return _db.UserSymbols.Any(e => e.Id == id);
    }
}

