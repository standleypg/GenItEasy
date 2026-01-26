using GenItEasy.Configuration;
using GenItEasy.Utilities;

namespace GenItEasy.Core.Tests.Utilities;

public class PathResolverTests
{
    [Fact]
    public void ResolveOutputPath_WithAbsolutePath_ReturnsAbsolutePath()
    {
        // Arrange
        var absolutePath = OperatingSystem.IsWindows() ? @"C:\output" : "/output";
        var config = new TypeScriptGenConfig
        {
            OutputPath = absolutePath,
            OutputFileName = "types.ts"
        };
        var resolver = new PathResolver(config);

        // Act
        var result = resolver.ResolveOutputPath();

        // Assert
        var expectedPath = Path.GetFullPath(Path.Combine(absolutePath, "types.ts"));
        Assert.Equal(expectedPath, result);
    }

    [Fact]
    public void ResolveOutputPath_WithRelativePath_ResolvesRelativeToCurrentDirectory()
    {
        // Arrange
        var config = new TypeScriptGenConfig
        {
            OutputPath = "output",
            OutputFileName = "models.ts"
        };
        var resolver = new PathResolver(config);

        // Act
        var result = resolver.ResolveOutputPath();

        // Assert
        var expectedPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "output", "models.ts"));
        Assert.Equal(expectedPath, result);
    }

    [Fact]
    public void ResolveOutputPath_WithNestedRelativePath_ResolvesCorrectly()
    {
        // Arrange
        var config = new TypeScriptGenConfig
        {
            OutputPath = "./src/generated/types",
            OutputFileName = "api.ts"
        };
        var resolver = new PathResolver(config);

        // Act
        var result = resolver.ResolveOutputPath();

        // Assert
        var expectedPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "src", "generated", "types", "api.ts"));
        Assert.Equal(expectedPath, result);
    }

    [Fact]
    public void ResolveOutputPath_WithParentDirectoryPath_ResolvesCorrectly()
    {
        // Arrange
        var config = new TypeScriptGenConfig
        {
            OutputPath = "../output",
            OutputFileName = "types.ts"
        };
        var resolver = new PathResolver(config);

        // Act
        var result = resolver.ResolveOutputPath();

        // Assert
        var expectedPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "output", "types.ts"));
        Assert.Equal(expectedPath, result);
    }

    [Fact]
    public void ResolveOutputPath_CombinesOutputPathAndFileName()
    {
        // Arrange
        var config = new TypeScriptGenConfig
        {
            OutputPath = "generated",
            OutputFileName = "custom-types.d.ts"
        };
        var resolver = new PathResolver(config);

        // Act
        var result = resolver.ResolveOutputPath();

        // Assert
        Assert.EndsWith("custom-types.d.ts", result);
        Assert.Contains("generated", result);
    }

    [Fact]
    public void ResolveOutputPath_ReturnsFullPath()
    {
        // Arrange
        var config = new TypeScriptGenConfig
        {
            OutputPath = "output",
            OutputFileName = "types.ts"
        };
        var resolver = new PathResolver(config);

        // Act
        var result = resolver.ResolveOutputPath();

        // Assert
        Assert.True(Path.IsPathRooted(result));
    }
}
