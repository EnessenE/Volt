namespace Volt.Models;

public class Chat
{
    public List<ChatMessage> Messages { get; set; }
    public List<Account> Members { get; set; }
}