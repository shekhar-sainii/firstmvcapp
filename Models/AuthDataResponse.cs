namespace FirstMvcApp.Models;

/// <summary>
/// Data structure for authentication responses (login/register)
/// </summary>
public class AuthDataResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}
