namespace Ratatosk;

public class MessageContext
{
    public IDictionary<string, object?> Data { get; }
        = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

    public MessageContext()
    {
    }

    public MessageContext(params (string key, object? value)[] entries)
    {
        foreach (var (key, value) in entries)
            this[key] = value;
    }

    public MessageContext With(string key, object? value)
    {
        ValidateKey(key);
        Data[key] = value;
        return this;
    }

    public object? this[string key]
    {
        get => Data.TryGetValue(key, out var v) ? v : null;
        set
        {
            ValidateKey(key);
            Data[key] = value;
        }
    }

    public static void ValidateKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (key.Contains(' '))
            throw new ArgumentException($"Key '{key}' must not contain spaces.", nameof(key));
    }
}
