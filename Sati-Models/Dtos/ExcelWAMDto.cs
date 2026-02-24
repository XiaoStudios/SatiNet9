namespace Sati_Models.DTOs;

public class ExcelWamDto
{
    public string WAMName { get; set; }
    public int Factor { get; set; }
    public double Value { get; set; }
    public string Time { get; set; }
    public double WaValue { get; set; }
    public double WamValue { get; set; }
    public double DifferenceWAWAM { get; set; } //difference between WA and WAM
}