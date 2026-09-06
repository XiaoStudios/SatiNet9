namespace Sati_Net_Last.API.Services;

public interface IJwtService
{
    string GenerateToken(int userId, string username, string email, string fullName);
    bool ValidateToken(string token);
}
