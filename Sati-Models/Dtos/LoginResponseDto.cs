namespace Sati_Models.DTOs;

public class LoginResponseDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // Token or session data returned by the API for the admin panel to create a cookie
    public string SessionToken { get; set; } = string.Empty;

    // Additional fields kept for backward compatibility
    public bool IsAdmin { get; set; }
    public List<string> Symbols { get; set; } = new();
}
