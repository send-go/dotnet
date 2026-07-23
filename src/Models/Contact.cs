using System.Text.Json.Serialization;

namespace Sendgo.Models;

/// <summary>수신자 정보.</summary>
public record Contact
{
    [JsonPropertyName("contact")]
    public required string PhoneNumber { get; init; }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    [JsonPropertyName("var1")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Var1 { get; init; }
    [JsonPropertyName("var2")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Var2 { get; init; }
    [JsonPropertyName("var3")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Var3 { get; init; }
    [JsonPropertyName("var4")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Var4 { get; init; }
    [JsonPropertyName("var5")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Var5 { get; init; }
    [JsonPropertyName("var6")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Var6 { get; init; }
    [JsonPropertyName("var7")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Var7 { get; init; }
    [JsonPropertyName("var8")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Var8 { get; init; }
}
