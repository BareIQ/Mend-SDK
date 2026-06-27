using System;
using System.Collections.Generic;
using System.Linq;
using Mend.Sdk.Projects.Models;

namespace Mend.Tool.Services;

internal static class ProjectResolver
{
    public static IReadOnlyList<Project> FindMatches(IReadOnlyList<Project> projects, string name)
    {
        return projects
            .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
