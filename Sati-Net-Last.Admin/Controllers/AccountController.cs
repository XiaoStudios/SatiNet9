using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sati_Models.DTOs;
using Sati_Net_Last.Admin.Repositories;
using Sati_Net_Last.Admin.Models;

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

        var resp = await _repo.AutenticarAsync(model.Correo, model.Password);
        if (resp == null)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña inválidos.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, resp.UserId.ToString()),
            new Claim(ClaimTypes.Name, resp.FullName ?? resp.Username),
            new Claim(ClaimTypes.Email, resp.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var props = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true
        };

        // Además, crear una cookie adicional o guardar el token en claims si es necesario
        if (!string.IsNullOrEmpty(resp.SessionToken))
        {
            identity.AddClaim(new Claim("SessionToken", resp.SessionToken));
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl!);

        return RedirectToAction("Index", "MtapiSettings");
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

