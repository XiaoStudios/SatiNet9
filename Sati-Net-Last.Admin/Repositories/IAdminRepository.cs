using Sati_Net_Last.Admin.Models;
using System.Threading.Tasks;

namespace Sati_Net_Last.Admin.Repositories;

public interface IAdminRepository
{
    // Devuelve el Admin si autenticación OK, o null si falla
    Task<AdminUser?> AutenticarAsync(string correo, string password);
}
