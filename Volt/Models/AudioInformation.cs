namespace Volt.Models;

public class AudioInformation
{
    public Account Speaker { get; set; }
    public Dictionary<int, float> AudioData { get; set; }
}