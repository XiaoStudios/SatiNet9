using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Sati_Models.DTOs;
using Sati_Net_Last.API.Repositories.Interfaces;

namespace Sati_Net_Last.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController (IAuthRepository authRepository, ILogger<AuthController> logger)
    {
        _authRepository = authRepository;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new
            {
                message = "Correo electrónico y contraseña son requeridos"
            });
        
        var email = request.Email.Trim();
        var passwordHash = ComputeSha256Hex(request.Password);
        var user = await _authRepository.GetEnabledUserByEmailAsync(email);

        if (user == null || !string.Equals(user.PasswordHash, passwordHash, StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { message = "Correo o contraseña incorrectos" });
        
        var symbols = await _authRepository.GetActiveSymbolsByUserIdAsync(user.Id);
        await _authRepository.UpdateLastLoginAsync(user.Id);

        var response = new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName ?? user.Username,
            IsAdmin = user.IsAdmin,
            Symbols = symbols
        };

        return Ok(response);
    }

    private static string ComputeSha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);

        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        
        return sb.ToString();
    }
}