using Microsoft.AspNetCore.Mvc;
using Sati_Net_Last.API.Repositories.Interfaces;

namespace Sati_Net_Last.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExcelController : ControllerBase
{
    private readonly ILogger<ExcelController> _logger;
    private readonly IExcelRepository _excelRepository;

    public ExcelController
    (
        ILogger<ExcelController> logger,
        IExcelRepository excelRepository
    )
    {
        _logger = logger;
        _excelRepository = excelRepository;
    }

    [HttpPost("load-excel-file")]
    public async Task<IActionResult> LoadExcelFile(IFormFile excelFile)
    {
        try
        {
            var wamData = await _excelRepository.GetWamData(excelFile);
            return Ok(wamData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the Excel file.");
            return StatusCode(500, "Internal server error while processing the Excel file.");
        }
    }
}
