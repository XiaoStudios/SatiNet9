namespace Sati_Models.DTOs;

public class UserSymbolDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SymbolStr { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
    public string? UserFullName { get; set; }
}
