namespace FirstMvcApp.Models;

public class ProfileViewModel
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public bool IsAuthenticated { get; set; }
}
