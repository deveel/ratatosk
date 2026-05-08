internal static class SampleConsolePrompts
{
    public static string Select(string prompt, params string[] choices)
        => Select(prompt, choices, choices.First());

    public static string Select(string prompt, IReadOnlyList<string> choices, string defaultValue)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            for (var i = 0; i < choices.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {choices[i]}");
            }

            Console.Write($"Select an option [{defaultValue}]: ");
            var input = Console.ReadLine()?.Trim();
            Console.WriteLine();

            if (String.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }

            if (Int32.TryParse(input, out var index) && index >= 1 && index <= choices.Count)
            {
                return choices[index - 1];
            }

            var selected = choices.FirstOrDefault(x => String.Equals(x, input, StringComparison.OrdinalIgnoreCase));
            if (selected is not null)
            {
                return selected;
            }

            Console.WriteLine("Please choose one of the listed options.");
            Console.WriteLine();
        }
    }

    public static string RequiredText(string prompt, string? defaultValue = null)
    {
        while (true)
        {
            var value = OptionalText(prompt, defaultValue);
            if (!String.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            Console.WriteLine("A value is required.");
            Console.WriteLine();
        }
    }

    public static string? OptionalText(string prompt, string? defaultValue = null)
    {
        Console.Write(prompt);
        if (!String.IsNullOrWhiteSpace(defaultValue))
        {
            Console.Write($" [{defaultValue}]");
        }

        Console.Write(": ");
        var input = Console.ReadLine();
        Console.WriteLine();

        return String.IsNullOrWhiteSpace(input)
            ? defaultValue
            : input.Trim();
    }

    public static bool Confirm(string prompt, bool defaultValue = true)
    {
        while (true)
        {
            Console.Write($"{prompt} [{(defaultValue ? "Y/n" : "y/N")}]: ");
            var input = Console.ReadLine()?.Trim();
            Console.WriteLine();

            if (String.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }

            if (input.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (input.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Console.WriteLine("Please answer yes or no.");
            Console.WriteLine();
        }
    }

    public static int RequiredInt(string prompt, int defaultValue)
    {
        while (true)
        {
            var value = OptionalText(prompt, defaultValue.ToString());
            if (Int32.TryParse(value, out var parsed))
            {
                return parsed;
            }

            Console.WriteLine("Please enter a valid integer.");
            Console.WriteLine();
        }
    }

    public static double RequiredDouble(string prompt, double defaultValue)
    {
        while (true)
        {
            var value = OptionalText(prompt, defaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (Double.TryParse(
                value,
                System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed))
            {
                return parsed;
            }

            Console.WriteLine("Please enter a valid number using '.' as decimal separator.");
            Console.WriteLine();
        }
    }
}
