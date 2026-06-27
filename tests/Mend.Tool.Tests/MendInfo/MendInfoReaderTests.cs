using System.IO;
using Mend.Tool.MendInfo;
using Xunit;
using MendInfoModel = Mend.Tool.MendInfo.MendInfo;

namespace Mend.Tool.Tests.MendInfoTests;

public sealed class MendInfoReaderTests
{
    private static string WriteTempFile(string json)
    {
        var dir  = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, ".mend-info"), json);
        return dir;
    }

    [Fact]
    public void TryRead_WhenFileHasProjectId_ReturnsProjectId()
    {
        var dir = WriteTempFile("""{ "projectId": "abc-123" }""");
        var result = MendInfoReader.TryRead(dir);
        Assert.NotNull(result);
        Assert.Equal("abc-123", result!.ProjectId);
        Assert.Null(result.ProjectName);
    }

    [Fact]
    public void TryRead_WhenFileHasProjectName_ReturnsProjectName()
    {
        var dir = WriteTempFile("""{ "projectName": "MyApp" }""");
        var result = MendInfoReader.TryRead(dir);
        Assert.NotNull(result);
        Assert.Equal("MyApp", result!.ProjectName);
        Assert.Null(result.ProjectId);
    }

    [Fact]
    public void TryRead_WhenFileHasBoth_ReturnsBoth()
    {
        var dir = WriteTempFile("""{ "projectId": "uuid-1", "projectName": "MyApp" }""");
        var result = MendInfoReader.TryRead(dir);
        Assert.NotNull(result);
        Assert.Equal("uuid-1", result!.ProjectId);
        Assert.Equal("MyApp", result.ProjectName);
    }

    [Fact]
    public void TryRead_WhenFileIsMissing_ReturnsNull()
    {
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        var result = MendInfoReader.TryRead(dir);
        Assert.Null(result);
    }

    [Fact]
    public void IsValid_WhenNeitherFieldPresent_ReturnsFalse()
    {
        var info = new MendInfoModel();
        Assert.False(info.IsValid);
    }

    [Fact]
    public void IsValid_WhenProjectIdPresent_ReturnsTrue()
    {
        var info = new MendInfoModel { ProjectId = "some-uuid" };
        Assert.True(info.IsValid);
    }

    [Fact]
    public void IsValid_WhenProjectNamePresent_ReturnsTrue()
    {
        var info = new MendInfoModel { ProjectName = "SomeProject" };
        Assert.True(info.IsValid);
    }
}
