using GenItEasy.Utilities;

namespace GenItEasy.Core.Tests.Utilities;

public class ConfigLoaderTests : IDisposable
{
    private readonly List<string> _tempFiles = [];

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
        GC.SuppressFinalize(this);
    }

    private string CreateTempConfigFile(string content)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
        File.WriteAllText(tempFile, content);
        _tempFiles.Add(tempFile);
        return tempFile;
    }

    [Fact]
    public void LoadConfig_WithValidConfig_ReturnsConfig()
    {
        // Arrange
        var json = """
        {
            "assemblyName": "Test.dll",
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "Test.Models" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act
        var config = ConfigLoader.LoadConfig(configPath);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("Test.dll", config.AssemblyName);
        Assert.Equal("./output", config.OutputPath);
        Assert.Equal("types.ts", config.OutputFileName);
        Assert.Single(config.Namespaces);
    }

    [Fact]
    public void LoadConfig_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "does_not_exist.json");

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => ConfigLoader.LoadConfig(nonExistentPath));
        Assert.Contains("Configuration file not found", exception.Message);
    }

    [Fact]
    public void LoadConfig_WithMissingAssemblyName_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "Test.Models" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => ConfigLoader.LoadConfig(configPath));
        Assert.Contains("AssemblyName must be specified", exception.Message);
    }

    [Fact]
    public void LoadConfig_WithEmptyAssemblyName_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
            "assemblyName": "   ",
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "Test.Models" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => ConfigLoader.LoadConfig(configPath));
        Assert.Contains("AssemblyName must be specified", exception.Message);
    }

    [Fact]
    public void LoadConfig_WithMissingOutputPath_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
            "assemblyName": "Test.dll",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "Test.Models" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => ConfigLoader.LoadConfig(configPath));
        Assert.Contains("OutputPath must be specified", exception.Message);
    }

    [Fact]
    public void LoadConfig_WithMissingOutputFileName_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
            "assemblyName": "Test.dll",
            "outputPath": "./output",
            "outputFileName": "",
            "namespaces": [
                { "namespace": "Test.Models" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => ConfigLoader.LoadConfig(configPath));
        Assert.Contains("OutputFileName must be specified", exception.Message);
    }

    [Fact]
    public void LoadConfig_WithEmptyNamespaces_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
            "assemblyName": "Test.dll",
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": []
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => ConfigLoader.LoadConfig(configPath));
        Assert.Contains("At least one namespace must be specified", exception.Message);
    }

    [Fact]
    public void LoadConfig_WithEmptyNamespaceName_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
            "assemblyName": "Test.dll",
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => ConfigLoader.LoadConfig(configPath));
        Assert.Contains("Namespace name cannot be empty", exception.Message);
    }

    [Fact]
    public void LoadConfig_WithJsonComments_ParsesSuccessfully()
    {
        // Arrange
        var json = """
        {
            // This is a comment
            "assemblyName": "Test.dll",
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "Test.Models" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act
        var config = ConfigLoader.LoadConfig(configPath);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("Test.dll", config.AssemblyName);
    }

    [Fact]
    public void LoadConfig_WithCaseInsensitivePropertyNames_ParsesSuccessfully()
    {
        // Arrange
        var json = """
        {
            "AssemblyName": "Test.dll",
            "OutputPath": "./output",
            "OutputFileName": "types.ts",
            "Namespaces": [
                { "Namespace": "Test.Models" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act
        var config = ConfigLoader.LoadConfig(configPath);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("Test.dll", config.AssemblyName);
    }

    [Fact]
    public void LoadConfig_WithMultipleNamespaces_ParsesAllNamespaces()
    {
        // Arrange
        var json = """
        {
            "assemblyName": "Test.dll",
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "Test.Models" },
                { "namespace": "Test.Entities" },
                { "namespace": "Test.DTOs" }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act
        var config = ConfigLoader.LoadConfig(configPath);

        // Assert
        Assert.NotNull(config);
        Assert.Equal(3, config.Namespaces.Count);
        Assert.Equal("Test.Models", config.Namespaces[0].Namespace);
        Assert.Equal("Test.Entities", config.Namespaces[1].Namespace);
        Assert.Equal("Test.DTOs", config.Namespaces[2].Namespace);
    }

    [Fact]
    public void LoadConfig_WithAllNamespaceOptions_ParsesSuccessfully()
    {
        // Arrange
        var json = """
        {
            "assemblyName": "Test.dll",
            "outputPath": "./output",
            "outputFileName": "types.ts",
            "namespaces": [
                {
                    "namespace": "Test.Models",
                    "includeTypes": ["User", "Product"],
                    "excludeTypes": ["Internal*"],
                    "includeGenericTypes": ["Result"],
                    "excludeGenericTypes": ["Cache*"],
                    "includeNested": true
                }
            ]
        }
        """;
        var configPath = CreateTempConfigFile(json);

        // Act
        var config = ConfigLoader.LoadConfig(configPath);

        // Assert
        Assert.NotNull(config);
        Assert.Single(config.Namespaces);
        var ns = config.Namespaces[0];
        Assert.Equal("Test.Models", ns.Namespace);
        Assert.NotNull(ns.IncludeTypes);
        Assert.Equal(2, ns.IncludeTypes.Count);
        Assert.NotNull(ns.ExcludeTypes);
        Assert.Single(ns.ExcludeTypes);
        Assert.True(ns.IncludeNested);
    }
}
