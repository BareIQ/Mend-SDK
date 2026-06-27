using System;
using System.Collections.Generic;
using System.Linq;
using Mend.Sdk.Applications;
using Mend.Sdk.Client;
using Mend.Sdk.Dependencies;
using Mend.Sdk.Dependencies.Models;
using Mend.Sdk.Extensions;
using Mend.Sdk.Projects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>(optional: true)
    .Build();

var services = new ServiceCollection();
services.AddMendSdk(config);
var provider = services.BuildServiceProvider();

var client = provider.GetRequiredService<IMendClient>();
var projectsClient = provider.GetRequiredService<IMendProjectsClient>();
var applicationClient = provider.GetRequiredService<IMendApplicationsClient>();
var dependenciesClient = provider.GetRequiredService<IMendDependenciesClient>();

Console.WriteLine("Mend SDK — Project Browser");
Console.WriteLine();

var projects = await projectsClient.GetProjectsAsync();

var applications = await applicationClient.GetApplicationsAsync();

if (projects.Count == 0)
{
    Console.WriteLine("No projects found for this organisation. Check your appsettings.json configuration.");
    return;
}

while (true)
{
    Console.WriteLine("Available projects:");
    for (var i = 0; i < projects.Count; i++)
        Console.WriteLine($"  {i + 1}. {projects[i].Name}");

    Console.WriteLine();
    Console.Write("Enter project number (or 'q' to quit): ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input) || input.Equals("q", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Goodbye.");
        break;
    }

    if (!int.TryParse(input, out var choice) || choice < 1 || choice > projects.Count)
    {
        Console.WriteLine($"Invalid input. Please enter a number between 1 and {projects.Count}.");
        Console.WriteLine();
        continue;
    }

    var selectedProject = projects[choice - 1];
    Console.WriteLine();

    var findings = await dependenciesClient.GetDependencySecurityFindingsAsync(selectedProject.Uuid);
    PrintFindingsTable(selectedProject.Name, findings.Where(f => f.Status == "ACTIVE").ToList());
    Console.WriteLine();
}

static void PrintFindingsTable(string projectName, IReadOnlyList<SecurityFinding> findings)
{
    static int SeverityRank(string severity) => severity.ToUpperInvariant() switch
    {
        "CRITICAL" => 0,
        "HIGH"     => 1,
        "MEDIUM"   => 2,
        "LOW"      => 3,
        _          => 4
    };

    var sorted = findings.OrderBy(f => SeverityRank(f.Severity)).ToList();

    const string sep = "──────────────────────────────────────────────────────────────────";

    Console.WriteLine($"Vulnerabilities for: {projectName}");
    Console.WriteLine(sep);
    Console.WriteLine($" {"#",-4} {"Severity",-10} {"CVE",-18} {"Library",-24} Status");
    Console.WriteLine(sep);

    if (sorted.Count == 0)
    {
        Console.WriteLine("No vulnerabilities found for this project.");
        Console.WriteLine(sep);
        return;
    }

    for (var i = 0; i < sorted.Count; i++)
    {
        var f = sorted[i];
        Console.WriteLine($" {i + 1,-4} {f.Severity,-10} {f.CveName,-18} {f.LibraryName,-24} {f.Status}");
    }

    Console.WriteLine(sep);

    var critical = sorted.Count(f => string.Equals(f.Severity, "CRITICAL", StringComparison.OrdinalIgnoreCase));
    var high     = sorted.Count(f => string.Equals(f.Severity, "HIGH",     StringComparison.OrdinalIgnoreCase));
    var medium   = sorted.Count(f => string.Equals(f.Severity, "MEDIUM",   StringComparison.OrdinalIgnoreCase));
    var low      = sorted.Count(f => string.Equals(f.Severity, "LOW",      StringComparison.OrdinalIgnoreCase));

    Console.WriteLine($"Total: {sorted.Count} finding(s)  [Critical: {critical}  High: {high}  Medium: {medium}  Low: {low}]");
}
