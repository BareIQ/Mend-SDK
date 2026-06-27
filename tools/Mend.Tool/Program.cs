using System;
using System.CommandLine;
using Mend.Sdk.Exceptions;
using Mend.Sdk.Extensions;
using Mend.Tool.Config;
using Mend.Tool.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var setupOption        = new Option<bool>("--setup")          { Description = "Configure Mend credentials interactively" };
var listProjectsOption = new Option<bool>("--list-projects")   { Description = "List all projects in the organisation" };
var projectIdOption    = new Option<string?>("--project-id")   { Description = "Show vulnerabilities for a project by UUID" };
var projectNameOption  = new Option<string?>("--project-name") { Description = "Show vulnerabilities for a project by name (supports partial, case-insensitive match)" };

var root = new RootCommand("mendcli — Mend security vulnerability viewer");
root.Add(setupOption);
root.Add(listProjectsOption);
root.Add(projectIdOption);
root.Add(projectNameOption);

root.SetAction(async parseResult =>
{
    var setup        = parseResult.GetValue(setupOption);
    var listProjects = parseResult.GetValue(listProjectsOption);
    var projectId    = parseResult.GetValue(projectIdOption);
    var projectName  = parseResult.GetValue(projectNameOption);

    if (setup)
        return await SetupHandler.RunAsync();

    ServiceProvider provider;
    try
    {
        var config   = MendCliConfig.Load();
        var services = new ServiceCollection();
        services.AddMendSdk(config);
        provider = services.BuildServiceProvider();
    }
    catch (Exception)
    {
        AnsiConsole.MarkupLine($"[red]Error:[/] Config file not found at [grey]{MendCliConfig.ConfigPath}[/]");
        AnsiConsole.MarkupLine("[grey]Run [bold]mendcli --setup[/] to configure your credentials.[/]");
        return 1;
    }

    await using (provider)
    {
        try
        {
            var vulnHandler = new VulnerabilityHandler(
                provider.GetRequiredService<Mend.Sdk.Projects.IMendProjectsClient>(),
                provider.GetRequiredService<Mend.Sdk.Dependencies.IMendDependenciesClient>());

            if (listProjects)
            {
                var handler = new ListProjectsHandler(provider.GetRequiredService<Mend.Sdk.Projects.IMendProjectsClient>());
                return await handler.RunAsync();
            }

            if (!string.IsNullOrEmpty(projectId))
                return await vulnHandler.RunByIdAsync(projectId);

            if (!string.IsNullOrEmpty(projectName))
                return await vulnHandler.RunByNameAsync(projectName);

            return await vulnHandler.RunFromMendInfoAsync();
        }
        catch (MendAuthException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Authentication failed for [grey]{ex.EndpointPath}[/].");
            AnsiConsole.MarkupLine($"[grey]Check credentials at {MendCliConfig.ConfigPath} or run [bold]mendcli --setup[/].[/]");
            return 1;
        }
        catch (MendApiException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] API error {(int)ex.StatusCode}: {Markup.Escape(ex.Message)}");
            return 1;
        }
    }
});

return await root.Parse(args).InvokeAsync();
