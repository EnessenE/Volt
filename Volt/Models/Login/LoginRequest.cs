namespace Volt.Models.Login;

public class LoginRequest
{
    public string Username { get; init; }
    public string EncryptedPassword { get; init; }
}