namespace Volt.Models;

public class Account
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public int Discriminator { get; set; }

    public override string ToString()
    {
        return $"{Id} - {Username}#{Discriminator}";
    }
}