using System.Text.Json;
using GenItEasy.Configuration;

namespace GenItEasy.Core.Tests.Configuration;

public class TypeScriptGenConfigTests
{
    [Fact]
    public void TypeScriptGenConfig_HasDefaultValues()
    {
        // Arrange & Act
        var config = new TypeScriptGenConfig();

        // Assert
        Assert.Null(config.AssemblyName);
        Assert.Null(config.Assemblies);
        Assert.Equal(string.Empty, config.OutputPath);
        Assert.Equal("models.gen.ts", config.OutputFileName);
        Assert.Null(config.BaseDirectory);
        Assert.False(config.IncludeStaticClasses);
        Assert.NotNull(config.Namespaces);
        Assert.Empty(config.Namespaces);
    }

    [Fact]
    public void TypeScriptGenConfig_CanSetProperties()
    {
        // Arrange
        var config = new TypeScriptGenConfig
        {
            AssemblyName = "Test.dll",
            OutputPath = "./output",
            OutputFileName = "custom.ts",
            BaseDirectory = "/base",
            IncludeStaticClasses = true,
            Namespaces = [new NamespaceConfig { Namespace = "Test.Namespace" }]
        };

        // Assert
        Assert.Equal("Test.dll", config.AssemblyName);
        Assert.Equal("./output", config.OutputPath);
        Assert.Equal("custom.ts", config.OutputFileName);
        Assert.Equal("/base", config.BaseDirectory);
        Assert.True(config.IncludeStaticClasses);
        Assert.Single(config.Namespaces);
        Assert.Equal("Test.Namespace", config.Namespaces[0].Namespace);
    }

    [Fact]
    public void TypeScriptGenConfig_DeserializesFromJson()
    {
        // Arrange
        var json = """
        {
            "assemblyName": "MyAssembly.dll",
            "outputPath": "./generated",
            "outputFileName": "types.ts",
            "baseDirectory": "/app",
            "includeStaticClasses": true,
            "namespaces": [
                {
                    "namespace": "MyApp.Models",
                    "includeNested": true
                }
            ]
        }
        """;

        // Act
        var config = JsonSerializer.Deserialize<TypeScriptGenConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(config);
        Assert.Equal("MyAssembly.dll", config.AssemblyName);
        Assert.Equal("./generated", config.OutputPath);
        Assert.Equal("types.ts", config.OutputFileName);
        Assert.Equal("/app", config.BaseDirectory);
        Assert.True(config.IncludeStaticClasses);
        Assert.Single(config.Namespaces);
        Assert.Equal("MyApp.Models", config.Namespaces[0].Namespace);
        Assert.True(config.Namespaces[0].IncludeNested);
    }
}

public class NamespaceConfigTests
{
    [Fact]
    public void NamespaceConfig_HasDefaultValues()
    {
        // Arrange & Act
        var config = new NamespaceConfig();

        // Assert
        Assert.Equal(string.Empty, config.Namespace);
        Assert.Null(config.IncludeTypes);
        Assert.Null(config.ExcludeTypes);
        Assert.Null(config.IncludeGenericTypes);
        Assert.Null(config.ExcludeGenericTypes);
        Assert.False(config.IncludeNested);
    }

    [Fact]
    public void NamespaceConfig_CanSetAllProperties()
    {
        // Arrange
        var config = new NamespaceConfig
        {
            Namespace = "Test.Namespace",
            IncludeTypes = ["TypeA", "TypeB"],
            ExcludeTypes = ["TypeC"],
            IncludeGenericTypes = ["Result*"],
            ExcludeGenericTypes = ["Internal*"],
            IncludeNested = true
        };

        // Assert
        Assert.Equal("Test.Namespace", config.Namespace);
        Assert.NotNull(config.IncludeTypes);
        Assert.Equal(2, config.IncludeTypes.Count);
        Assert.NotNull(config.ExcludeTypes);
        Assert.Single(config.ExcludeTypes);
        Assert.NotNull(config.IncludeGenericTypes);
        Assert.Single(config.IncludeGenericTypes);
        Assert.NotNull(config.ExcludeGenericTypes);
        Assert.Single(config.ExcludeGenericTypes);
        Assert.True(config.IncludeNested);
    }

    [Fact]
    public void NamespaceConfig_DeserializesFromJson()
    {
        // Arrange
        var json = """
        {
            "namespace": "MyApp.Entities",
            "includeTypes": ["User", "Product"],
            "excludeTypes": ["Internal*"],
            "includeGenericTypes": ["Result"],
            "excludeGenericTypes": ["Cache*"],
            "includeNested": true
        }
        """;

        // Act
        var config = JsonSerializer.Deserialize<NamespaceConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(config);
        Assert.Equal("MyApp.Entities", config.Namespace);
        Assert.NotNull(config.IncludeTypes);
        Assert.Equal(2, config.IncludeTypes.Count);
        Assert.Contains("User", config.IncludeTypes);
        Assert.Contains("Product", config.IncludeTypes);
        Assert.NotNull(config.ExcludeTypes);
        Assert.Single(config.ExcludeTypes);
        Assert.True(config.IncludeNested);
    }
}

public class TypeScriptGenConfigAssembliesTests
{
    [Fact]
    public void TypeScriptGenConfig_CanSetAssembliesProperty()
    {
        // Arrange & Act
        var config = new TypeScriptGenConfig
        {
            Assemblies = ["ProjectA", "ProjectB", "ProjectC"],
            OutputPath = "./output",
            OutputFileName = "types.ts",
            Namespaces = [new NamespaceConfig { Namespace = "ProjectA.Models" }]
        };

        // Assert
        Assert.Null(config.AssemblyName);
        Assert.NotNull(config.Assemblies);
        Assert.Equal(3, config.Assemblies.Count);
        Assert.Equal("ProjectA", config.Assemblies[0]);
        Assert.Equal("ProjectB", config.Assemblies[1]);
        Assert.Equal("ProjectC", config.Assemblies[2]);
    }

    [Fact]
    public void TypeScriptGenConfig_DeserializesAssembliesFromJson()
    {
        // Arrange
        var json = """
        {
            "assemblies": ["MyApp.Models", "MyApp.Shared", "MyApp.Core"],
            "outputPath": "./generated",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "MyApp.Models" }
            ]
        }
        """;

        // Act
        var config = JsonSerializer.Deserialize<TypeScriptGenConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(config);
        Assert.Null(config.AssemblyName);
        Assert.NotNull(config.Assemblies);
        Assert.Equal(3, config.Assemblies.Count);
        Assert.Equal("MyApp.Models", config.Assemblies[0]);
        Assert.Equal("MyApp.Shared", config.Assemblies[1]);
        Assert.Equal("MyApp.Core", config.Assemblies[2]);
    }

    [Fact]
    public void TypeScriptGenConfig_DeserializesSingleAssemblyFromJson()
    {
        // Arrange - backward compatibility test
        var json = """
        {
            "assemblyName": "MyApp.Models",
            "outputPath": "./generated",
            "outputFileName": "types.ts",
            "namespaces": [
                { "namespace": "MyApp.Models" }
            ]
        }
        """;

        // Act
        var config = JsonSerializer.Deserialize<TypeScriptGenConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(config);
        Assert.Equal("MyApp.Models", config.AssemblyName);
        Assert.Null(config.Assemblies);
    }
}
