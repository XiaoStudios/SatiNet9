namespace Sati_Models.DTOs;

public class WamDto
{
    public int NWaValue { get; set; }
    public double WAValue { get; set; } //weighted average
    public int NWamValue { get; set; }
    public double WamValue { get; set; } //weighted average moving
    public List<ExcelWamDto> WamValues { get; set; }
}