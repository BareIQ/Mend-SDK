using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Mend.Sdk.Exceptions;
using Mend.Sdk.Extensions;
using Mend.Tool.Config;
using Mend.Tool.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var setupOption        = new Option<bool>("--setup",         "Configure Mend credentials interactively");
var listProjectsOption = new Option<bool>("--list-projects",  "List all projects in the organisation");
var projectIdOption    = new Option<string?>("--project-id",  "Show vulnerabilities for a project by UUID");
var projectNameOption  = new Option<string?>("--project-name","Show vulnerabilities for a project by name (supports partial, case-insensitive match)");

var root = new RootCommand("mendcli — Mend security vulnerability viewer");
root.AddOption(setupOption);
root.AddOption(listProjectsOption);
root.AddOption(projectIdOption);
root.AddOption(projectNameOption);

root.SetHandler(async (InvocationContext context) =>
{
    var setup       = context.ParseResult.GetValueForOption(setupOption);
    var listProjects = context.ParseResult.GetValueForOption(listProjectsOption);
    var projectId   = context.ParseResult.GetValueForOption(projectIdOption);
    var projectName = context.ParseResult.GetValueForOption(projectNameOption);

    if (setup)
    {
        context.ExitCode = await SetupHandler.RunAsync();
        return;
    }

    // All other commands require ~/.mendcli/config.json
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
        context.ExitCode = 1;
        return;
    }

    await using (provider)
    {
        try
        {
            var vulnHandler = new VulnerabilityHandler(
                provider.GetRequiredService<Mend.Sdk.Projects.IMendProjectsClient>(),
                provider.GetRequiredService<Mend.Sdk.Dependencies.IMendDependenciesClient>());

            int exitCode;
            if (listProjects)
            {
                var handler = new ListProjectsHandler(provider.GetRequiredService<Mend.Sdk.Projects.IMendProjectsClient>());
                exitCode = await handler.RunAsync();
            }
            else if (!string.IsNullOrEmpty(projectId))
            {
                exitCode = await vulnHandler.RunByIdAsync(projectId);
            }
            else if (!string.IsNullOrEmpty(projectName))
            {
                exitCode = await vulnHandler.RunByNameAsync(projectName);
            }
            else
            {
                exitCode = await vulnHandler.RunFromMendInfoAsync();
            }

            context.ExitCode = exitCode;
        }
        catch (MendAuthException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Authentication failed for [grey]{ex.EndpointPath}[/].");
            AnsiConsole.MarkupLine($"[grey]Check credentials at {MendCliConfig.ConfigPath} or run [bold]mendcli --setup[/].[/]");
            context.ExitCode = 1;
        }
        catch (MendApiException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] API error {(int)ex.StatusCode}: {Markup.Escape(ex.Message)}");
            context.ExitCode = 1;
        }
    }
});

return await root.InvokeAsync(args);
