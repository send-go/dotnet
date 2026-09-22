using System.Text.Json;

namespace Sendgo.Exceptions;

/// <summary>Sendgo API 호출 실패 시 발생하는 예외.</summary>
public class SendgoException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }
    public string Endpoint { get; }
    public string ApiVersion { get; }
    public object? ResponseBody { get; }

    public SendgoException(string message, int statusCode = 0, string? errorCode = null,
        string endpoint = "", string apiVersion = "", object? responseBody = null)
        : base(message)
    {
        StatusCode   = statusCode;
        ErrorCode    = errorCode;
        Endpoint     = endpoint;
        ApiVersion   = apiVersion;
        ResponseBody = responseBody;
    }

    internal static string? ReadString(Dictionary<string, object?> body, string key) =>
        body.GetValueOrDefault(key) switch
        {
            string text => text,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            _ => null,
        };

    internal static SendgoException FromResponse(
        int status, Dictionary<string, object?> body, string endpoint, string apiVersion)
    {
        var errorCode    = ReadString(body, "code");
        var errorMessage = ReadString(body, "message") ?? "Unknown error";
        var message      = $"HTTP {status}{(errorCode != null ? $" [{errorCode}]" : "")} {errorMessage}";
        return new SendgoException(message, status, errorCode, endpoint, apiVersion, body);
    }
}
