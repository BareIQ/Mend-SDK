using System.Collections.Generic;
using Mend.Sdk.Projects.Models;
using Mend.Tool.Services;
using Xunit;

namespace Mend.Tool.Tests.Services;

public sealed class ProjectResolverTests
{
    private static IReadOnlyList<Project> SampleProjects() =>
    [
        new Project { Uuid = "1", Name = "BoardsService" },
        new Project { Uuid = "2", Name = "CardsViewService" },
        new Project { Uuid = "3", Name = "OrganizationsService" },
        new Project { Uuid = "4", Name = "Clarizen.Modernization.Api" },
        new Project { Uuid = "5", Name = "clarizen-core" },
    ];

    [Fact]
    public void FindMatches_ExactCaseSensitive_ReturnsSingleResult()
    {
        var matches = ProjectResolver.FindMatches(SampleProjects(), "BoardsService");
        Assert.Single(matches);
        Assert.Equal("1", matches[0].Uuid);
    }

    [Fact]
    public void FindMatches_PartialCaseInsensitive_ReturnsMatches()
    {
        var matches = ProjectResolver.FindMatches(SampleProjects(), "boards");
        Assert.Single(matches);
        Assert.Equal("BoardsService", matches[0].Name);
    }

    [Fact]
    public void FindMatches_UpperCase_MatchesLowerCaseName()
    {
        var matches = ProjectResolver.FindMatches(SampleProjects(), "CLARIZEN");
        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public void FindMatches_NoMatch_ReturnsEmptyList()
    {
        var matches = ProjectResolver.FindMatches(SampleProjects(), "zzznomatch");
        Assert.Empty(matches);
    }

    [Fact]
    public void FindMatches_MultipleMatches_ReturnsAll()
    {
        var matches = ProjectResolver.FindMatches(SampleProjects(), "service");
        Assert.Equal(3, matches.Count);
    }

    [Fact]
    public void FindMatches_EmptyProjectList_ReturnsEmptyList()
    {
        var matches = ProjectResolver.FindMatches([], "anything");
        Assert.Empty(matches);
    }

    [Fact]
    public void FindMatches_EmptyName_ReturnsAll()
    {
        var matches = ProjectResolver.FindMatches(SampleProjects(), string.Empty);
        Assert.Equal(5, matches.Count);
    }
}
