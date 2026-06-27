using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Mend.Sdk.Extensions;
using Mend.Sdk.Projects;
using Mend.Tool.Config;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Mend.Tool.Handlers;

internal static class SetupHandler
{
    public static async Task<int> RunAsync()
    {
        AnsiConsole.MarkupLine("[bold]mendcli setup[/] — configure your Mend credentials\n");

        var baseUrl = AnsiConsole.Prompt(
            new TextPrompt<string>("Base URL:")
                .DefaultValue("https://api-saas.whitesourcesoftware.com"));

        var orgUuid = AnsiConsole.Ask<string>("Organisation UUID:");
        var email   = AnsiConsole.Ask<string>("Email:");
        var userKey = AnsiConsole.Prompt(new TextPrompt<string>("User key:").Secret());

        var valid = false;
        await AnsiConsole.Status().StartAsync("Validating credentials...", async _ =>
        {
            try
            {
                var config   = MendCliConfig.BuildInMemory(baseUrl, orgUuid, email, userKey);
                var services = new ServiceCollection();
                services.AddMendSdk(config);
                await using var provider = services.BuildServiceProvider();

                var client = provider.GetRequiredService<IMendProjectsClient>();
                await client.GetProjectsAsync(pageSize: 1);
                valid = true;
            }
            catch (Exception)
            {
                // validation failed — handled below
            }
        });

        if (!valid)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Authentication failed. Check your credentials and try again.");
            return 1;
        }

        var configDir = Path.GetDirectoryName(MendCliConfig.ConfigPath)!;
        Directory.CreateDirectory(configDir);

        var configJson = JsonSerializer.Serialize(new
        {
            Mend = new
            {
                BaseUrl  = baseUrl,
                OrgUuid  = orgUuid,
                Email    = email,
                UserKey  = userKey
            }
        }, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(MendCliConfig.ConfigPath, configJson);

        AnsiConsole.MarkupLine($"[green]✓[/] Configuration saved to [grey]{MendCliConfig.ConfigPath}[/]");
        return 0;
    }
}
