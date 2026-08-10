using Microsoft.EntityFrameworkCore;
using Sati_Net_Last.Admin.Data;
using Sati_Net_Last.Admin.Models;
using System.Threading.Tasks;

namespace Sati_Net_Last.Admin.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly AdminDbContext _db;

    public AdminRepository(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<AdminUser?> AutenticarAsync(string correo, string password)
    {
        var admin = await _db.Admins
            .Include(a => a.Rol)
            .FirstOrDefaultAsync(a => a.Correo == correo);

        if (admin == null) return null;

        bool verified = BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
        if (!verified) return null;

        return admin;
    }
}