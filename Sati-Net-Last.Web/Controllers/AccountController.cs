using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Sati_Models.DTOs;

namespace Sati_Net_Last.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        // Si ya hay una sesión activa, no mostrar nuevamente el login.
        if (HttpContext.Session.GetString("UserId") != null)
            return RedirectToAction("Index", "Home");

        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        if (!ModelState.IsValid ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            ViewBag.Error = "Ingresa tu usuario y contraseña.";
            return View(request);
        }

        try
        {
            // Recupera el HttpClient registrado como "BackendAPI" en Program.cs.
            var client = _httpClientFactory.CreateClient("BackendAPI");

            // Envía el LoginRequestDto como JSON a POST /api/auth/login.
            var response = await client.PostAsJsonAsync("api/auth/login", request);

            // Credenciales inválidas: API devuelve 401.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View(request);
            }

            // Error de validación u otro error controlado desde API.
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "El API rechazó el login de {Username}. Status: {StatusCode}",
                    request.Username,
                    response.StatusCode);

                ViewBag.Error = "No fue posible validar el acceso. Intenta nuevamente.";
                return View(request);
            }

            // Lee el JSON que devolvió el API y lo convierte a LoginResponseDto.
            var loginResponse = await response.Content
                .ReadFromJsonAsync<LoginResponseDto>();

            if (loginResponse == null)
            {
                _logger.LogError(
                    "El API devolvió una respuesta de login vacía para {Username}.",
                    request.Username);

                ViewBag.Error = "La respuesta del servidor no es válida.";
                return View(request);
            }

            // Guardar datos mínimos del usuario en Session.
            HttpContext.Session.SetString("UserId", loginResponse.UserId.ToString());
            HttpContext.Session.SetString("Username", loginResponse.Username);
            HttpContext.Session.SetString("FullName", loginResponse.FullName);
            HttpContext.Session.SetString("IsAdmin", loginResponse.IsAdmin.ToString());

            // Una lista se serializa como JSON antes de guardarla en Session.
            var symbolsJson = JsonSerializer.Serialize(loginResponse.Symbols);
            HttpContext.Session.SetString("UserSymbols", symbolsJson);

            return RedirectToAction("Index", "Home");
        }
        catch (HttpRequestException ex)
        {
            // El Web no pudo comunicarse con el API.
            _logger.LogError(
                ex,
                "No fue posible conectar con BackendAPI durante el login de {Username}.",
                request.Username);

            ViewBag.Error = "No se pudo conectar con el servidor de autenticación.";
            return View(request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAjax([FromForm] LoginRequestDto request)
    {
        if (!ModelState.IsValid ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { success = false, message = "Ingresa tu usuario y contraseña." });
        }

        try
        {
            var client = _httpClientFactory.CreateClient("BackendAPI");
            var response = await client.PostAsJsonAsync("api/auth/login", request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return Unauthorized(new { success = false, message = "Usuario o contraseña incorrectos." });
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("El API rechazó el login de {Username}. Status: {StatusCode}",
                    request.Username, response.StatusCode);

                return StatusCode((int)response.StatusCode, new
                {
                    success = false,
                    message = "No fue posible validar el acceso."
                });
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (loginResponse == null)
            {
                return StatusCode(500, new { success = false, message = "Respuesta de autenticación inválida." });
            }

            HttpContext.Session.SetString("UserId", loginResponse.UserId.ToString());
            HttpContext.Session.SetString("Username", loginResponse.Username);
            HttpContext.Session.SetString("FullName", loginResponse.FullName);
            HttpContext.Session.SetString("IsAdmin", loginResponse.IsAdmin.ToString());
            HttpContext.Session.SetString("UserSymbols", JsonSerializer.Serialize(loginResponse.Symbols));

            return Ok(new
            {
                success = true,
                redirectUrl = Url.Action("Index", "Home")
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "No fue posible conectar con BackendAPI durante el login de {Username}.",
                request.Username);

            return StatusCode(500, new
            {
                success = false,
                message = "No se pudo conectar con el servidor de autenticación."
            });
        }
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}