using Sati_Models.DTOs;

namespace Sati_Net_Last.API.Repositories.Interfaces;

public interface IExcelRepository
{
    public Task<WamDto> GetWamData(IFormFile excelFile);
}
