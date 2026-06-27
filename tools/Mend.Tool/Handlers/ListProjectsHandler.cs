using System.Threading.Tasks;
using Mend.Sdk.Projects;
using Mend.Tool.Display;
using Spectre.Console;

namespace Mend.Tool.Handlers;

internal sealed class ListProjectsHandler
{
    private readonly IMendProjectsClient _projectsClient;

    public ListProjectsHandler(IMendProjectsClient projectsClient)
    {
        _projectsClient = projectsClient;
    }

    public async Task<int> RunAsync()
    {
        var projects = await AnsiConsole.Status()
            .StartAsync("Fetching projects...", _ => _projectsClient.GetProjectsAsync(pageSize: 10000));

        ProjectTable.Render(projects);
        return 0;
    }
}
