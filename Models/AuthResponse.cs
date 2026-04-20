namespace FirstMvcApp.Models;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Token { get; set; }
    public UserDto User { get; set; }
}

public class UserDto
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}
