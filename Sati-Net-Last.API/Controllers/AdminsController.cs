using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sati_Models.DBModels;

namespace Sati_Net_Last.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminsController : ControllerBase
{
    private readonly SatiDevContext _db;

    public AdminsController(SatiDevContext db)
    {
        _db = db;
    }

    // GET: api/Admins
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var list = await _db.AdminUsers.AsNoTracking().ToListAsync();
        var result = list.Select(u => new { u.Id, FullName = u.FullName ?? u.Username }).ToList();
        return Ok(result);
    }
}
