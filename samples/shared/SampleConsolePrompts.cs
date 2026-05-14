using Spectre.Console;

internal static class SampleConsolePrompts
{
    public static string Select(string prompt, params string[] choices)
        => Select(prompt, choices, choices.First());

    public static string Select(string prompt, IReadOnlyList<string> choices, string defaultValue)
    {
        var selection = new SelectionPrompt<string>()
            .Title(prompt)
            .PageSize(10)
            .MoreChoicesText("[dim](move up and down to reveal more options)[/]")
            .AddChoices(choices);

        return AnsiConsole.Prompt(selection);
    }

    public static string RequiredText(string prompt, string? defaultValue = null)
    {
        var textPrompt = new TextPrompt<string>($"[bold]{prompt}[/]")
            .Validate(value =>
            {
                if (string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(defaultValue))
                    return ValidationResult.Error("[red]A value is required.[/]");
                return ValidationResult.Success();
            });

        if (!string.IsNullOrWhiteSpace(defaultValue))
            textPrompt.DefaultValueStyle(new Style(foreground: Color.Default))
                      .DefaultValue(defaultValue);

        return AnsiConsole.Prompt(textPrompt);
    }

    public static string? OptionalText(string prompt, string? defaultValue = null)
    {
        var textPrompt = new TextPrompt<string>($"[bold]{prompt}[/]")
            .AllowEmpty();

        if (!string.IsNullOrWhiteSpace(defaultValue))
            textPrompt.DefaultValueStyle(new Style(foreground: Color.Default))
                      .DefaultValue(defaultValue);

        var result = AnsiConsole.Prompt(textPrompt);
        return string.IsNullOrWhiteSpace(result) ? defaultValue : result.Trim();
    }

    public static bool Confirm(string prompt, bool defaultValue = true)
        => AnsiConsole.Confirm($"[bold]{prompt}[/]", defaultValue);

    public static int RequiredInt(string prompt, int defaultValue)
        => AnsiConsole.Prompt(
            new TextPrompt<int>($"[bold]{prompt}[/]")
                .DefaultValue(defaultValue)
                .DefaultValueStyle(new Style(foreground: Color.Default))
                .ValidationErrorMessage("[red]Please enter a valid integer.[/]"));

    public static double RequiredDouble(string prompt, double defaultValue)
        => AnsiConsole.Prompt(
            new TextPrompt<double>($"[bold]{prompt}[/]")
                .DefaultValue(defaultValue)
                .DefaultValueStyle(new Style(foreground: Color.Default))
                .ValidationErrorMessage("[red]Please enter a valid number using '.' as decimal separator.[/]"));

    public static string MultiLineBody(string prompt, string? initial = null)
    {
        if (!string.IsNullOrEmpty(initial))
            AnsiConsole.MarkupLine($"[dim]Current body:[/] [italic]{initial}[/]");

        AnsiConsole.MarkupLine($"[bold]{prompt}[/]");
        AnsiConsole.MarkupLine("[dim](type [cyan]!done[/] on a new line to finish, or leave empty to keep current)[/]");

        var lines = new List<string>();
        while (true)
        {
            var line = System.Console.ReadLine();
            if (line == null || line.Trim().Equals("!done", System.StringComparison.OrdinalIgnoreCase))
                break;
            lines.Add(line);
        }

        var text = string.Join("\n", lines);
        if (string.IsNullOrWhiteSpace(text) && !string.IsNullOrEmpty(initial))
            return initial;

        return text;
    }
}
