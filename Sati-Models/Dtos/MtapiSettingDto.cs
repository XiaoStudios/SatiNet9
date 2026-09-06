namespace Sati_Models.DTOs;

public class MtapiSettingDto
{
    public int Id { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string MtUser { get; set; } = string.Empty;
    public string MtPassword { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? LastConnectionStatus { get; set; }
    public DateTime? LastConnectionAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
