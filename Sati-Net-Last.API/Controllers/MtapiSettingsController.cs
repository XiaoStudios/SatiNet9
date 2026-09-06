using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sati_Models.DBModels;
using Sati_Models.DTOs;

namespace Sati_Net_Last.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MtapiSettingsController : ControllerBase
{
    private readonly SatiDevContext _db;

    public MtapiSettingsController(SatiDevContext db)
    {
        _db = db;
    }

    // GET: api/MtapiSettings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MtapiSettingDto>>> GetAll()
    {
        var list = await _db.MtapiSettings.AsNoTracking().ToListAsync();
        var dto = list.Select(m => new MtapiSettingDto
        {
            Id = m.Id,
            Host = m.Host,
            Port = m.Port,
            MtUser = m.MtUser,
            MtPassword = m.MtPassword,
            IsActive = m.IsActive ?? false,
            LastConnectionStatus = m.LastConnectionStatus,
            LastConnectionAt = m.LastConnectionAt
        }).ToList();

        return Ok(dto);
    }

    // GET: api/MtapiSettings/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MtapiSettingDto>> Get(int id)
    {
        var m = await _db.MtapiSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (m == null) return NotFound();
        var dto = new MtapiSettingDto
        {
            Id = m.Id,
            Host = m.Host,
            Port = m.Port,
            MtUser = m.MtUser,
            MtPassword = m.MtPassword,
            IsActive = m.IsActive ?? false,
            LastConnectionStatus = m.LastConnectionStatus,
            LastConnectionAt = m.LastConnectionAt
        };
        return Ok(dto);
    }

    // POST: api/MtapiSettings
    [HttpPost]
    public async Task<ActionResult<MtapiSettingDto>> Create([FromBody] MtapiSettingDto dto)
    {
        var model = new MtapiSetting
        {
            Host = dto.Host,
            Port = dto.Port,
            MtUser = dto.MtUser,
            MtPassword = dto.MtPassword,
            IsActive = dto.IsActive,
            LastConnectionStatus = dto.LastConnectionStatus,
            LastConnectionAt = dto.LastConnectionAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.MtapiSettings.Add(model);
        await _db.SaveChangesAsync();
        dto.Id = model.Id;
        return CreatedAtAction(nameof(Get), new { id = model.Id }, dto);
    }

    // PUT: api/MtapiSettings/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MtapiSettingDto dto)
    {
        if (id != dto.Id) return BadRequest();
        var model = await _db.MtapiSettings.FirstOrDefaultAsync(x => x.Id == id);
        if (model == null) return NotFound();
        model.Host = dto.Host;
        model.Port = dto.Port;
        model.MtUser = dto.MtUser;
        model.MtPassword = dto.MtPassword;
        model.IsActive = dto.IsActive;
        model.LastConnectionStatus = dto.LastConnectionStatus;
        model.LastConnectionAt = dto.LastConnectionAt;
        model.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/MtapiSettings/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _db.MtapiSettings.FirstOrDefaultAsync(x => x.Id == id);
        if (model == null) return NotFound();
        _db.MtapiSettings.Remove(model);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
