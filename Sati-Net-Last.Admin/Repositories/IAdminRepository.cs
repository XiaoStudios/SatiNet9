using Sati_Models.DTOs;
using System.Threading.Tasks;

namespace Sati_Net_Last.Admin.Repositories;

public interface IAdminRepository
{
    // Devuelve el LoginResponseDto si autenticación OK, o null si falla
    Task<LoginResponseDto?> AutenticarAsync(string correo, string password);
}
