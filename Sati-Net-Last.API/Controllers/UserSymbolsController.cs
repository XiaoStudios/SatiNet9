using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sati_Models.DBModels;
using Sati_Models.DTOs;

namespace Sati_Net_Last.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserSymbolsController : ControllerBase
{
    private readonly SatiDevContext _db;

    public UserSymbolsController(SatiDevContext db)
    {
        _db = db;
    }

    // GET: api/UserSymbols?userId=5  OR header X-User-Id
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSymbolDto>>> Get([FromQuery] int? userId)
    {
        var header = Request.Headers["X-User-Id"].FirstOrDefault();
        int? uid = userId;
        if (uid == null && !string.IsNullOrEmpty(header) && int.TryParse(header, out var parsed)) uid = parsed;

        var query = _db.UserSymbols.AsNoTracking().AsQueryable();
        if (uid != null)
            query = query.Where(s => s.UserId == uid);

        var list = await query.ToListAsync();

        var dto = list.Select(s => new UserSymbolDto
        {
            Id = s.Id,
            UserId = s.UserId,
            SymbolStr = s.SymbolStr,
            IsActive = s.IsActive ?? false,
            AssignedAt = s.AssignedAt,
            UserFullName = s.User != null ? s.User.FullName : null
        }).ToList();

        return Ok(dto);
    }

    // GET: api/UserSymbols/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserSymbolDto>> GetById(int id)
    {
        var s = await _db.UserSymbols.AsNoTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return NotFound();
        var dto = new UserSymbolDto
        {
            Id = s.Id,
            UserId = s.UserId,
            SymbolStr = s.SymbolStr,
            IsActive = s.IsActive ?? false,
            AssignedAt = s.AssignedAt,
            UserFullName = s.User?.FullName
        };
        return Ok(dto);
    }

    // POST: api/UserSymbols
    [HttpPost]
    public async Task<ActionResult<UserSymbolDto>> Create([FromBody] UserSymbolDto dto)
    {
        var model = new UserSymbol
        {
            UserId = dto.UserId,
            SymbolStr = dto.SymbolStr,
            IsActive = dto.IsActive,
            AssignedAt = dto.AssignedAt == default ? DateTime.UtcNow : dto.AssignedAt
        };
        _db.UserSymbols.Add(model);
        await _db.SaveChangesAsync();
        dto.Id = model.Id;
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, dto);
    }

    // PUT: api/UserSymbols/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserSymbolDto dto)
    {
        if (id != dto.Id) return BadRequest();
        var model = await _db.UserSymbols.FirstOrDefaultAsync(x => x.Id == id);
        if (model == null) return NotFound();
        model.UserId = dto.UserId;
        model.SymbolStr = dto.SymbolStr;
        model.IsActive = dto.IsActive;
        model.AssignedAt = dto.AssignedAt;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/UserSymbols/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _db.UserSymbols.FirstOrDefaultAsync(x => x.Id == id);
        if (model == null) return NotFound();
        _db.UserSymbols.Remove(model);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
