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
        Console.WriteLine("\n--- 🔍 INICIANDO DIAGNÓSTICO DE LOGIN ---");
        Console.WriteLine($"Intentando entrar con correo: '{correo}'");

        var admin = await _db.Admins
            .FirstOrDefaultAsync(a => a.Email == correo);

        if (admin == null)
        {
            Console.WriteLine("❌ RECHAZO 1: El correo NO existe en la base de datos MySQL.");
            return null;
        }

        Console.WriteLine($"✅ Usuario encontrado: {admin.FullName ?? admin.Username}");
        Console.WriteLine($"-> Estado IsEnabled: {admin.IsEnabled}");
        Console.WriteLine($"-> Estado IsAdmin: {admin.IsAdmin}");
        Console.WriteLine($"-> Hash guardado: {admin.PasswordHash}");

        if (!admin.IsEnabled)
        {
            Console.WriteLine("❌ RECHAZO 2: La cuenta está deshabilitada (IsEnabled = false).");
            return null;
        }

        if (!admin.IsAdmin)
        {
            Console.WriteLine("❌ RECHAZO 3: El usuario NO es administrador (IsAdmin = false).");
            return null;
        }
        string hashPerfecto = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine($"\n🔥 EL HASH PERFECTO PARA '{password}' ES: {hashPerfecto}\n");

        bool verified = BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
        if (!verified)
        {
            Console.WriteLine("❌ RECHAZO 4: La contraseña 'admin123' no cuadra con el Hash.");
            return null;
        }

        Console.WriteLine("✅ ÉXITO TOTAL: Todas las puertas se abrieron.");
        return admin;
    }
}