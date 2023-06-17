namespace Volt.Models.Signup;

public class SignupRequest
{
    public string Username { get; set; }
    public string EncryptedPassword { get; set; }
}