using System.Collections.Generic;
using System.Linq;
using Mend.Sdk.Projects.Models;
using Spectre.Console;

namespace Mend.Tool.Display;

internal static class ProjectTable
{
    public static void Render(IReadOnlyList<Project> projects)
    {
        var table = new Table()
            .Border(TableBorder.Square)
            .ShowRowSeparators()
            .AddColumn(new TableColumn("[bold]UUID[/]"))
            .AddColumn(new TableColumn("[bold]Name[/]"));

        foreach (var p in projects.OrderBy(p => p.Name))
            table.AddRow(Markup.Escape(p.Uuid), Markup.Escape(p.Name));

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[grey]{projects.Count} project(s)[/]");
    }
}
