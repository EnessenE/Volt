namespace Volt.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }
        public byte[] EncryptedMessage { get; set; }

        public Account Sender { get; set; }

        public DateTime Created { get; set; }

        public Account Receiver { get; set; }
        public DateTime? LastUpdated { get; set; }

        public override string ToString()
        {
            return $"{Sender} > {Receiver} - {Id}";
        }
    }
}
