using System.ComponentModel.DataAnnotations;
using Deveel;
using Ratatosk;
using Spectre.Console;

using AnnotationsValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

internal static class SampleOutputHelper
{
    public static void PrintSchema(string name, IChannelSchema schema)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[bold]Property[/]").Width(20))
            .AddColumn(new TableColumn("[bold]Value[/]"));

        table.AddRow("Name", $"[cyan]{Markup.Escape(schema.DisplayName ?? name)}[/]");
        table.AddRow("Identity", Markup.Escape(schema.GetLogicalIdentity() ?? "—"));
        table.AddRow("Capabilities", Markup.Escape(schema.Capabilities.ToString()));
        table.AddRow("Content Types", Markup.Escape(JoinOrNone(schema.ContentTypes.Select(x => x.ToString()))));
        table.AddRow("Parameters", Markup.Escape(JoinOrNone(schema.Parameters.Select(x => $"{x.Name}:{x.DataType}{(x.IsRequired ? "*" : "")}"))));
        table.AddRow("Endpoints", Markup.Escape(JoinOrNone(schema.Endpoints.Select(x => $"{x.Type}(send={x.CanSend},receive={x.CanReceive},required={x.IsRequired})"))));
        table.AddRow("Message Properties", Markup.Escape(JoinOrNone(schema.MessageProperties.Select(x => x.Name))));

        AnsiConsole.Write(new Panel(table)
            .Header($"[bold yellow]{Markup.Escape(name)}[/]", Justify.Center)
            .Border(BoxBorder.Double));
        AnsiConsole.WriteLine();
    }

    public static void PrintValidationResult(string label, IEnumerable<AnnotationsValidationResult> results)
    {
        var errors = results.ToList();
        if (errors.Count == 0)
        {
            AnsiConsole.MarkupLine($"[bold]{Markup.Escape(label)}[/]: [green]:heavy_check_mark: valid[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold]{Markup.Escape(label)}[/]: [red]:cross_mark: invalid[/]");
            foreach (var error in errors)
            {
                AnsiConsole.MarkupLine($"  [red]:arrow_right: {Markup.Escape(error.ErrorMessage ?? "Unknown error")}[/]");
            }
        }
    }

    public static void PrintResult<T>(string label, OperationResult<T> result)
    {
        if (result.IsSuccess())
            AnsiConsole.MarkupLine($"[bold]{Markup.Escape(label)}[/]: [green]:heavy_check_mark: ok[/]");
        else
            AnsiConsole.MarkupLine($"[bold]{Markup.Escape(label)}[/]: [red]:cross_mark: {Markup.Escape(result.Error?.Code ?? "ERROR")} - {Markup.Escape(result.Error?.Message ?? "Unknown error")}[/]");
    }

    public static void PrintSendResult(string label, OperationResult<SendResult> result)
    {
        if (!result.IsSuccess() || result.Value is null)
        {
            PrintResult(label, result);
            return;
        }

        var r = result.Value;
        AnsiConsole.MarkupLine($"[bold]{Markup.Escape(label)}[/]: local=[cyan]{Markup.Escape(r.MessageId)}[/], remote=[cyan]{Markup.Escape(r.RemoteMessageId)}[/], status=[green]{Markup.Escape(r.Status.ToString())}[/]");
    }

    public static string JoinOrNone(IEnumerable<string> values)
    {
        var items = values.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        return items.Length == 0 ? "none" : string.Join(", ", items);
    }

    public static void AddIfPresent(ConnectionSettings settings, string parameterName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            settings.SetParameter(parameterName, value);
        }
    }

    public static string ReadFileOrDefault(string? file, string defaultValue)
        => string.IsNullOrWhiteSpace(file) ? defaultValue : File.ReadAllText(file);
}
