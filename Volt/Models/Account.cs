using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace Volt.Models;

public class Account
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public int Discriminator { get; set; }
    public string Password { get; set; }
    public ConcurrentDictionary<string, ConnectionData> Connections { get; set; } = new ConcurrentDictionary<string, ConnectionData>();
    public ConcurrentDictionary<string, ConnectionData> VoiceConnections { get; set; } = new ConcurrentDictionary<string, ConnectionData>();

    /// <summary>
    /// Secured by by your password!
    /// </summary>
    public string SecuredKey { get; set; }

    public bool IsSystem { get; set; }

    public override string ToString()
    {
        return $"{Id} - {Username}#{Discriminator}";
    }
}