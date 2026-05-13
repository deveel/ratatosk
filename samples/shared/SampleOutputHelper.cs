using System.ComponentModel.DataAnnotations;
using Deveel;
using Deveel.Messaging;

internal static class SampleOutputHelper
{
    public static void PrintSchema(string name, IChannelSchema schema)
    {
        Console.WriteLine($"[{name}] {schema.DisplayName}");
        Console.WriteLine($"  Identity: {schema.GetLogicalIdentity()}");
        Console.WriteLine($"  Capabilities: {schema.Capabilities}");
        Console.WriteLine($"  Content Types: {JoinOrNone(schema.ContentTypes.Select(x => x.ToString()))}");
        Console.WriteLine($"  Parameters: {JoinOrNone(schema.Parameters.Select(x => $"{x.Name}:{x.DataType}{(x.IsRequired ? "*" : "")}"))}");
        Console.WriteLine($"  Endpoints: {JoinOrNone(schema.Endpoints.Select(x => $"{x.Type}(send={x.CanSend},receive={x.CanReceive},required={x.IsRequired})"))}");
        Console.WriteLine($"  Message Properties: {JoinOrNone(schema.MessageProperties.Select(x => x.Name))}");
        Console.WriteLine();
    }

    public static void PrintValidationResult(string label, IEnumerable<ValidationResult> results)
    {
        var errors = results.ToList();
        Console.WriteLine($"{label}: {(errors.Count == 0 ? "valid" : "invalid")}");
        foreach (var error in errors)
        {
            Console.WriteLine($"  - {error.ErrorMessage}");
        }
    }

    public static void PrintResult<T>(string label, OperationResult<T> result)
    {
        Console.WriteLine($"{label}: {(result.IsSuccess() ? "ok" : $"{result.Error?.Code} - {result.Error?.Message}")}");
    }

    public static void PrintSendResult(string label, OperationResult<SendResult> result)
    {
        if (!result.IsSuccess() || result.Value is null)
        {
            PrintResult(label, result);
            return;
        }

        Console.WriteLine($"{label}: local={result.Value.MessageId}, remote={result.Value.RemoteMessageId}, status={result.Value.Status}");
    }

    public static string JoinOrNone(IEnumerable<string> values)
    {
        var items = values.Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
        return items.Length == 0 ? "none" : String.Join(", ", items);
    }

    public static void AddIfPresent(ConnectionSettings settings, string parameterName, string? value)
    {
        if (!String.IsNullOrWhiteSpace(value))
        {
            settings.SetParameter(parameterName, value);
        }
    }

    public static string ReadFileOrDefault(string? file, string defaultValue)
        => String.IsNullOrWhiteSpace(file) ? defaultValue : File.ReadAllText(file);
}
