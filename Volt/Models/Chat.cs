namespace Volt.Models;

public class Chat
{
    public List<ChatMessage> Messages { get; set; }
    public Account Sender { get; set; }
    public Account Receiver { get; set; }
}