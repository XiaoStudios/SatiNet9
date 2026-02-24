using OfficeOpenXml;
using Sati_Models.DTOs;
using Sati_React.Server.Repositories.Interfaces;

namespace Sati_React.Server.Repositories.Implementations;

public class ExcelRepository : IExcelRepository
{
    private readonly ILogger<ExcelRepository> _logger;

    public ExcelRepository(ILogger<ExcelRepository> logger)
    {
        _logger = logger;
    }

    public async Task<WamDto> GetWamData(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            _logger.LogError("Invalid Excel file provided.");
            throw new ArgumentException("Excel file cannot be null or empty.", nameof(excelFile));
        }

        try
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0]; // Read the first worksheet
            var rowCount = worksheet.Dimension.Rows;
            var colCount = worksheet.Dimension.Columns;
            // Console.WriteLine($"Row Count: {rowCount}, Column Count: {colCount}");
            // Implementation logic to read the Excel file and extract WAM data
            // This is a placeholder for the actual implementation
            var wamDataResult = new WamDto
            {
                WamValues = new List<ExcelWamDto>()
            };
            int waRowValues = 60;
            int startRow = 2;
            int endRow = startRow + waRowValues;

            for (int row = startRow; row < endRow; row++)
            {
                var rowData = new ExcelWamDto()
                {
                    WAMName = worksheet.Cells[row, 1].Text,
                    Factor = int.Parse(worksheet.Cells[row, 2].Text),
                    Value = double.Parse(worksheet.Cells[row, 3].Text),
                    Time = worksheet.Cells[row, 4].Text
                };

                wamDataResult.WamValues.Add(rowData);
            }

            int decimalsToRound = 3;

            //Weighted Average
            double totalValue = Math.Round(wamDataResult.WamValues.Sum(wam => wam.Value), decimalsToRound);
            double totalAverage = Math.Round(totalValue / wamDataResult.WamValues.Count, decimalsToRound);
            wamDataResult.NWaValue = waRowValues;
            wamDataResult.WAValue = totalAverage;

            //Weighted Average Moving
            int wamRowValue = 20;
            int wamNValue = Enumerable.Range(1, wamRowValue).Sum();
            int wamLoopRows = waRowValues - wamRowValue;
            // var factorValueLstTmp = new List<double>();
            var wamValueLstTmp = new double[wamRowValue];
            var waValueLstTmp = new double[wamRowValue];

            int loopTmp = 0;

            for (int i = wamRowValue; i <= waRowValues; i++)
            {
                // Console.WriteLine($"line{i}----------");
                loopTmp = 0;
                for (int j = i - wamRowValue; j < i; j++)
                {
                    var currentFactor = loopTmp + 1;
                    // Console.WriteLine($"line{j}");
                    // Console.WriteLine($"line{j} {currentFactor} * {result.WamValues[j].Value} with {Math.Round(currentFactor * result.WamValues[j].Value, 2)} and {wamNValue}");
                    wamValueLstTmp[loopTmp] = Math.Round(currentFactor * wamDataResult.WamValues[j].Value, 2);
                    waValueLstTmp[loopTmp] = wamDataResult.WamValues[j].Value;
                    loopTmp++;
                }

                wamDataResult.WamValues[i - 1].WaValue = Math.Round(waValueLstTmp.Sum() / wamRowValue, decimalsToRound);
                wamDataResult.WamValues[i - 1].WamValue = Math.Round(Math.Round(wamValueLstTmp.Sum(), 2) / wamNValue, decimalsToRound);
                wamDataResult.WamValues[i - 1].DifferenceWAWAM = Math.Round(wamDataResult.WamValues[i - 1].WamValue - wamDataResult.WamValues[wamRowValue].Value, decimalsToRound);
            }

            // Console.WriteLine($"Result Count: {System.Text.Json.JsonSerializer.Serialize(result)}");
            // result.DifferenceWAWAM = Math.Round(wamNValue - totalAverage, 2);
            wamDataResult.NWamValue = wamRowValue;
            wamDataResult.WamValue = wamNValue;

            return wamDataResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the Excel file.");
            throw new InvalidOperationException("An error occurred while processing the Excel file.", ex);
        }
    }

}