using System.Text.Json.Serialization;

namespace ClosetUIServerless.Models;

public class PartMeasu
{
    [JsonPropertyName("id")]
    public int ID { get; set; }
    public int Measure { get; set; }
}
