using Microsoft.EntityFrameworkCore;
using Sati_Models.DBModels;
using Sati_Net_Last.API.Data;
using Sati_Net_Last.API.Repositories.Interfaces;

namespace Sati_Net_Last.API.Repositories.Implementations;

public class AuthRepository : IAuthRepository
{
    private readonly SatiDevContext _satiDevContext;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository(SatiDevContext satiDevContext, ILogger<AuthRepository> logger)
    {
        _satiDevContext = satiDevContext;
        _logger = logger;
    }

    public async Task<AdminUser?> GetEnabledUserByEmailAsync(string email)
    {
        return await _satiDevContext.AdminUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && u.IsEnabled == true);
    }

    public async Task<List<string>> GetActiveSymbolsByUserIdAsync(int userId)
    {
        return await _satiDevContext.UserSymbols
                .AsNoTracking()
                .Where(s => s.UserId == userId && s.IsActive == true)
                .Select(s => s.SymbolStr)
                .ToListAsync();
    }
    
    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await _satiDevContext.AdminUsers.FirstOrDefaultAsync(x => x.Id == userId);
        
        if (user == null)
            return;
        
        user.LastLoginAt = DateTime.UtcNow;
        await _satiDevContext.SaveChangesAsync();
    }
}