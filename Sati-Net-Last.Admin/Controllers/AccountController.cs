using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sati_Net_Last.Admin.Models;
using Sati_Net_Last.Admin.Repositories;

namespace Sati_Net_Last.Admin.Controllers;

public class AccountController : Controller
{
    private readonly IAdminRepository _repo;

    public AccountController(IAdminRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var admin = await _repo.AutenticarAsync(model.Correo, model.Password);
        if (admin == null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña inválidos.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.NombreAdmin),
            new Claim(ClaimTypes.Email, admin.Correo)
        };

        if (admin.Rol != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, admin.Rol.TipoRol));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var props = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl!);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ObtenerHash()
    {
        // Método temporal para generar hash de ejemplo para insertar en la base de datos.
        var hash = BCrypt.Net.BCrypt.HashPassword("admin123");
        return Content(hash);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }
}

