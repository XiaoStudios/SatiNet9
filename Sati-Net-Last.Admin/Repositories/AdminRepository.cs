using Sati_Models.DTOs;
using System.Net.Http.Json;

namespace Sati_Net_Last.Admin.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<AdminRepository> _logger;

    public AdminRepository(IHttpClientFactory httpFactory, ILogger<AdminRepository> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> AutenticarAsync(string correo, string password)
    {
        try
        {
            var client = _httpFactory.CreateClient("BackendAPI");
            var request = new LoginRequestDto { Email = correo, Password = password };
            var resp = await client.PostAsJsonAsync("api/Auth/login", request);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Autenticación fallida para {Email}: {Status}", correo, resp.StatusCode);
                return null;
            }

            var dto = await resp.Content.ReadFromJsonAsync<LoginResponseDto>();
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al autenticar contra la API para {Email}", correo);
            return null;
        }
    }
}
