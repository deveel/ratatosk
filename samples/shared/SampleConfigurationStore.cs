using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

internal sealed record SampleConfigurationField(string Key, string Prompt, bool IsSecret = false, bool IsRequired = false);

internal sealed class SampleConfigurationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string filePath;
    private JsonObject? root;

    public SampleConfigurationStore(string fileName = "appsettings.local.json")
    {
        filePath = ResolveConfigPath(fileName);
    }

    public string FilePath => filePath;

    public void ConfigureSection(string sectionName, params SampleConfigurationField[] fields)
    {
        var rootObject = LoadRoot();
        var section = GetOrCreateSection(rootObject, sectionName);

        Console.WriteLine($"{sectionName} configuration");
        Console.WriteLine($"Values are stored in: {filePath}");
        Console.WriteLine();

        foreach (var field in fields)
        {
            var current = section[field.Key]?.GetValue<string>();
            var value = PromptForValue(field, current);
            if (!String.IsNullOrWhiteSpace(value))
            {
                section[field.Key] = value;
            }
        }

        SaveRoot(rootObject);

        Console.WriteLine();
        Console.WriteLine($"Saved {sectionName} settings to {filePath}");
    }

    public string? GetValue(string sectionName, string key, string? environmentName = null)
    {
        if (!String.IsNullOrWhiteSpace(environmentName))
        {
            var environmentValue = Environment.GetEnvironmentVariable(environmentName);
            if (!String.IsNullOrWhiteSpace(environmentValue))
            {
                return environmentValue;
            }
        }

        return GetStoredValue(sectionName, key);
    }

    public bool HasValue(string sectionName, string key, string? environmentName = null)
        => !String.IsNullOrWhiteSpace(GetValue(sectionName, key, environmentName));

    public string GetRequired(string sectionName, string key, string environmentName)
        => GetValue(sectionName, key, environmentName)
            ?? throw new InvalidOperationException(
                $"Missing configuration value '{sectionName}:{key}'. Set {environmentName} or run the configure command for this sample.");

    public bool GetBool(string sectionName, string key, string environmentName, bool defaultValue)
    {
        var value = GetValue(sectionName, key, environmentName);
        return Boolean.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    private string? GetStoredValue(string sectionName, string key)
    {
        var rootObject = LoadRoot();
        if (rootObject[sectionName] is not JsonObject section)
        {
            return null;
        }

        return section[key]?.GetValue<string>();
    }

    private JsonObject LoadRoot()
    {
        if (root is not null)
        {
            return root;
        }

        if (!File.Exists(filePath))
        {
            root = new JsonObject();
            return root;
        }

        var json = File.ReadAllText(filePath);
        root = JsonNode.Parse(json) as JsonObject ?? new JsonObject();
        return root;
    }

    private void SaveRoot(JsonObject rootObject)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!String.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, rootObject.ToJsonString(JsonOptions) + Environment.NewLine);
        root = rootObject;
    }

    private static JsonObject GetOrCreateSection(JsonObject rootObject, string sectionName)
    {
        if (rootObject[sectionName] is JsonObject section)
        {
            return section;
        }

        section = new JsonObject();
        rootObject[sectionName] = section;
        return section;
    }

    private static string? PromptForValue(SampleConfigurationField field, string? current)
    {
        while (true)
        {
            Console.Write($"{field.Prompt}");
            Console.Write(field.IsRequired ? " (required" : " (optional");
            Console.Write(!String.IsNullOrWhiteSpace(current) ? ", leave empty to keep current)" : ")");

            if (!String.IsNullOrWhiteSpace(current))
            {
                Console.Write($" [{MaskValue(current, field.IsSecret)}]");
            }

            Console.Write(": ");

            var input = field.IsSecret ? ReadSecret() : Console.ReadLine();
            Console.WriteLine();

            if (String.IsNullOrWhiteSpace(input))
            {
                if (!String.IsNullOrWhiteSpace(current))
                {
                    return current;
                }

                if (!field.IsRequired)
                {
                    return null;
                }

                Console.WriteLine("A value is required.");
                Console.WriteLine();
                continue;
            }

            return input.Trim();
        }
    }

    private static string ReadSecret()
    {
        if (Console.IsInputRedirected)
        {
            return Console.ReadLine() ?? String.Empty;
        }

        var buffer = new StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0)
                {
                    buffer.Length--;
                }

                continue;
            }

            if (!Char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
            }
        }

        return buffer.ToString();
    }

    private static string MaskValue(string value, bool isSecret)
    {
        if (!isSecret)
        {
            return value;
        }

        return value.Length <= 4
            ? new string('*', value.Length)
            : $"{new string('*', value.Length - 4)}{value[^4..]}";
    }

    private static string ResolveConfigPath(string fileName)
    {
        foreach (var startPath in EnumerateStartPaths())
        {
            var directory = new DirectoryInfo(startPath);
            while (directory is not null)
            {
                if (directory.EnumerateFiles("*.csproj").Any())
                {
                    return Path.Combine(directory.FullName, fileName);
                }

                directory = directory.Parent;
            }
        }

        return Path.Combine(Environment.CurrentDirectory, fileName);
    }

    private static IEnumerable<string> EnumerateStartPaths()
    {
        yield return AppContext.BaseDirectory;

        var currentDirectory = Environment.CurrentDirectory;
        if (!String.Equals(currentDirectory, AppContext.BaseDirectory, StringComparison.OrdinalIgnoreCase))
        {
            yield return currentDirectory;
        }
    }
}
