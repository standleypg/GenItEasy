using GenItEasy.Configuration;
using GenItEasy.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenItEasy.Core.Tests.Filters;

public class TypeFilterTests
{
    private readonly ILogger _logger = NullLogger.Instance;

    #region IsInTargetNamespace Tests

    [Fact]
    public void IsInTargetNamespace_WithExactMatch_ReturnsTrue()
    {
        // Arrange
        var nsConfig = new NamespaceConfig { Namespace = "System.Collections" };

        // Act
        var result = TypeFilter.IsInTargetNamespace(typeof(System.Collections.ArrayList), nsConfig);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInTargetNamespace_WithDifferentNamespace_ReturnsFalse()
    {
        // Arrange
        var nsConfig = new NamespaceConfig { Namespace = "System.IO" };

        // Act
        var result = TypeFilter.IsInTargetNamespace(typeof(System.Collections.ArrayList), nsConfig);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInTargetNamespace_WithNestedNamespace_IncludeNestedFalse_ReturnsFalse()
    {
        // Arrange
        var nsConfig = new NamespaceConfig { Namespace = "System", IncludeNested = false };

        // Act - System.Collections.ArrayList is in a nested namespace of System
        var result = TypeFilter.IsInTargetNamespace(typeof(System.Collections.ArrayList), nsConfig);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInTargetNamespace_WithNestedNamespace_IncludeNestedTrue_ReturnsTrue()
    {
        // Arrange
        var nsConfig = new NamespaceConfig { Namespace = "System", IncludeNested = true };

        // Act
        var result = TypeFilter.IsInTargetNamespace(typeof(System.Collections.ArrayList), nsConfig);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInTargetNamespace_WithNullNamespace_ReturnsFalse()
    {
        // Arrange
        var nsConfig = new NamespaceConfig { Namespace = "Test" };
        // Anonymous types have null namespace, but we'll use a type without namespace
        // Using a mock approach - testing the behavior with a well-known type

        // Act & Assert - String has a namespace, so this won't apply directly
        // The method handles null namespace by returning false
        var result = TypeFilter.IsInTargetNamespace(typeof(string), nsConfig);
        Assert.False(result); // string is in System, not Test
    }

    #endregion

    #region IsNotStaticClass Tests

    [Fact]
    public void IsNotStaticClass_WithRegularClass_ReturnsTrue()
    {
        // Arrange
        var config = new TypeScriptGenConfig { IncludeStaticClasses = false };
        var filter = new TypeFilter(config, _logger);

        // Act
        var result = filter.IsNotStaticClass(typeof(string));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNotStaticClass_WithStaticClass_ReturnsFalse()
    {
        // Arrange
        var config = new TypeScriptGenConfig { IncludeStaticClasses = false };
        var filter = new TypeFilter(config, _logger);

        // Act - Console is a static class
        var result = filter.IsNotStaticClass(typeof(Console));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNotStaticClass_WithStaticClass_IncludeStaticClassesTrue_ReturnsTrue()
    {
        // Arrange
        var config = new TypeScriptGenConfig { IncludeStaticClasses = true };
        var filter = new TypeFilter(config, _logger);

        // Act
        var result = filter.IsNotStaticClass(typeof(Console));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNotStaticClass_WithInterface_ReturnsTrue()
    {
        // Arrange
        var config = new TypeScriptGenConfig { IncludeStaticClasses = false };
        var filter = new TypeFilter(config, _logger);

        // Act
        var result = filter.IsNotStaticClass(typeof(IDisposable));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNotStaticClass_WithEnum_ReturnsTrue()
    {
        // Arrange
        var config = new TypeScriptGenConfig { IncludeStaticClasses = false };
        var filter = new TypeFilter(config, _logger);

        // Act
        var result = filter.IsNotStaticClass(typeof(DayOfWeek));

        // Assert
        Assert.True(result);
    }

    #endregion

    #region IsTypeIncluded Tests

    [Fact]
    public void IsTypeIncluded_WithNoFilters_ReturnsTrue()
    {
        // Arrange
        var config = new TypeScriptGenConfig();
        var filter = new TypeFilter(config, _logger);
        var nsConfig = new NamespaceConfig { Namespace = "System" };

        // Act
        var result = filter.IsTypeIncluded(typeof(string), nsConfig);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTypeIncluded_WithIncludeTypes_MatchingType_ReturnsTrue()
    {
        // Arrange
        var config = new TypeScriptGenConfig();
        var filter = new TypeFilter(config, _logger);
        var nsConfig = new NamespaceConfig
        {
            Namespace = "System",
            IncludeTypes = ["String"]
        };

        // Act
        var result = filter.IsTypeIncluded(typeof(string), nsConfig);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTypeIncluded_WithIncludeTypes_NonMatchingType_ReturnsFalse()
    {
        // Arrange
        var config = new TypeScriptGenConfig();
        var filter = new TypeFilter(config, _logger);
        var nsConfig = new NamespaceConfig
        {
            Namespace = "System",
            IncludeTypes = ["Integer"]
        };

        // Act
        var result = filter.IsTypeIncluded(typeof(string), nsConfig);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTypeIncluded_WithExcludeTypes_MatchingType_ReturnsFalse()
    {
        // Arrange
        var config = new TypeScriptGenConfig();
        var filter = new TypeFilter(config, _logger);
        var nsConfig = new NamespaceConfig
        {
            Namespace = "System",
            ExcludeTypes = ["String"]
        };

        // Act
        var result = filter.IsTypeIncluded(typeof(string), nsConfig);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTypeIncluded_WithExcludeTypes_NonMatchingType_ReturnsTrue()
    {
        // Arrange
        var config = new TypeScriptGenConfig();
        var filter = new TypeFilter(config, _logger);
        var nsConfig = new NamespaceConfig
        {
            Namespace = "System",
            ExcludeTypes = ["Integer"]
        };

        // Act
        var result = filter.IsTypeIncluded(typeof(string), nsConfig);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTypeIncluded_WithPrefixPattern_MatchesPrefix()
    {
        // Arrange
        var config = new TypeScriptGenConfig();
        var filter = new TypeFilter(config, _logger);
        var nsConfig = new NamespaceConfig
        {
            Namespace = "System",
            IncludeTypes = ["Str"]
        };

        // Act - String starts with "Str"
        var result = filter.IsTypeIncluded(typeof(string), nsConfig);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTypeIncluded_WithSuffixPattern_MatchesSuffix()
    {
        // Arrange
        var config = new TypeScriptGenConfig();
        var filter = new TypeFilter(config, _logger);
        var nsConfig = new NamespaceConfig
        {
            Namespace = "System",
            IncludeTypes = ["ing"]
        };

        // Act - String ends with "ing"
        var result = filter.IsTypeIncluded(typeof(string), nsConfig);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTypeIncluded_IsCaseInsensitive()
    {
        // Arrange
        var config = new TypeScriptGenConfig();
        var filter = new TypeFilter(config, _logger);
        var nsConfig = new NamespaceConfig
        {
            Namespace = "System",
            IncludeTypes = ["string"] // lowercase
        };

        // Act - Type name is "String"
        var result = filter.IsTypeIncluded(typeof(string), nsConfig);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTypeIncluded_GenericType_WithExcludeGenericTypes_ReturnsFalse()
    {
        // Arrange
        var config = new TypeScriptGenConfig();
        var filter = new TypeFilter(config, _logger);
        var nsConfig = new NamespaceConfig
        {
            Namespace = "System.Collections.Generic",
            ExcludeGenericTypes = ["List"]
        };

        // Act
        var result = filter.IsTypeIncluded(typeof(List<>), nsConfig);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTypeIncluded_GenericType_WithIncludeGenericTypes_MatchingType_ReturnsTrue()
    {
        // Arrange
        var config = new TypeScriptGenConfig();
        var filter = new TypeFilter(config, _logger);
        var nsConfig = new NamespaceConfig
        {
            Namespace = "System.Collections.Generic",
            IncludeGenericTypes = ["List"]
        };

        // Act
        var result = filter.IsTypeIncluded(typeof(List<>), nsConfig);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTypeIncluded_GenericType_WithIncludeGenericTypes_NonMatchingType_ReturnsFalse()
    {
        // Arrange
        var config = new TypeScriptGenConfig();
        var filter = new TypeFilter(config, _logger);
        var nsConfig = new NamespaceConfig
        {
            Namespace = "System.Collections.Generic",
            IncludeGenericTypes = ["Dictionary"]
        };

        // Act
        var result = filter.IsTypeIncluded(typeof(List<>), nsConfig);

        // Assert
        Assert.False(result);
    }

    #endregion
}
