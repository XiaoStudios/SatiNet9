using Sati_Models.DBModels;

namespace Sati_Net_Last.API.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<AdminUser?> GetEnabledUserByEmailAsync(string email);
    Task<List<string>> GetActiveSymbolsByUserIdAsync(int userId);
    Task UpdateLastLoginAsync(int userId);
}