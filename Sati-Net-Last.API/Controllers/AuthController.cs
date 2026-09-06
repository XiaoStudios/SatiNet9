using Microsoft.AspNetCore.Mvc;
using Sati_Models.DTOs;
using Sati_Net_Last.API.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sati_Net_Last.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _config;

    public AuthController(IAuthRepository authRepository, ILogger<AuthController> logger, IConfiguration config)
    {
        _authRepository = authRepository;
        _logger = logger;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Correo electrónico y contraseña son requeridos" });

        var email = request.Email.Trim();
        var user = await _authRepository.GetEnabledUserByEmailAsync(email);

        if (user == null)
            return Unauthorized(new { message = "Correo o contraseña incorrectos" });

        // 🟢 AQUI ESTÁ EL CAMBIO: Verificación con SHA-256 en lugar de BCrypt
        string hashedInput = ComputeSha256Hash(request.Password);
        bool verified = string.Equals(hashedInput, user.PasswordHash, StringComparison.OrdinalIgnoreCase);

        if (!verified)
            return Unauthorized(new { message = "Correo o contraseña incorrectos" });

        var symbols = await _authRepository.GetActiveSymbolsByUserIdAsync(user.Id);

        // Fabricación del token criptográfico
        var jwtKey = _config["Jwt:Key"] ?? "TuSuperClaveSecretaSuperLargaYSeguraParaJWT2026_SATI";
        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("IsAdmin", user.IsAdmin.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        var response = new LoginResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName ?? user.Username,
            Email = user.Email,
            SessionToken = jwtString,
            IsAdmin = user.IsAdmin,
            Symbols = symbols ?? new List<string>()
        };

        await _authRepository.UpdateLastLoginAsync(user.Id);

        return Ok(response);
    }

    // 🟢 Método auxiliar SHA-256
    private static string ComputeSha256Hash(string rawData)
    {
        using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}