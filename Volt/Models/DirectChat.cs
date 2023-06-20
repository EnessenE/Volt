namespace Volt.Models;

public class DirectChat
{
    public List<ChatMessage> Messages { get; set; }
    public List<Account> Members { get; set; }

    public Account Receiver { get; set; }
    public Account Sender { get; set; }

    public string EncryptionKey { get; set; }

    public Guid Id { get; set; }

    public override string ToString()
    {
        return $"{Id} - Members: {Members?.Count} - Msgs: {Messages?.Count}";
    }
}