using GenItEasy.Formatters;
using GenItEasy.TypeScript.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenItEasy.Core.Tests.Formatters;

public class TypeFormatterTests
{
    private readonly TypeFormatter _formatter;

    public TypeFormatterTests()
    {
        _formatter = new TypeFormatter(NullLogger.Instance);
    }

    #region FormatMemberType - Type Mapping Tests

    [Fact]
    public void FormatMemberType_WithGuid_ReturnsString()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestTypeMappingClass>(nameof(TestTypeMappingClass.Id));

        // Act
        var result = _formatter.FormatMemberType(property, "Guid");

        // Assert
        Assert.Equal("string", result);
    }

    [Fact]
    public void FormatMemberType_WithSystemGuid_ReturnsString()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestTypeMappingClass>(nameof(TestTypeMappingClass.Id));

        // Act
        var result = _formatter.FormatMemberType(property, "System.Guid");

        // Assert
        Assert.Equal("string", result);
    }

    [Fact]
    public void FormatMemberType_WithDateTime_ReturnsDate()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestTypeMappingClass>(nameof(TestTypeMappingClass.CreatedAt));

        // Act
        var result = _formatter.FormatMemberType(property, "System.DateTime");

        // Assert
        Assert.Equal("Date", result);
    }

    [Fact]
    public void FormatMemberType_WithDateTimeOffset_ReturnsDate()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestTypeMappingClass>(nameof(TestTypeMappingClass.ModifiedAt));

        // Act
        var result = _formatter.FormatMemberType(property, "System.DateTimeOffset");

        // Assert
        Assert.Equal("Date", result);
    }

    [Fact]
    public void FormatMemberType_WithString_ReturnsString()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestTypeMappingClass>(nameof(TestTypeMappingClass.Name));

        // Act
        var result = _formatter.FormatMemberType(property, "string");

        // Assert
        Assert.Equal("string", result);
    }

    [Fact]
    public void FormatMemberType_WithNumber_ReturnsNumber()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestTypeMappingClass>(nameof(TestTypeMappingClass.Count));

        // Act
        var result = _formatter.FormatMemberType(property, "number");

        // Assert
        Assert.Equal("number", result);
    }

    [Fact]
    public void FormatMemberType_WithBoolean_ReturnsBoolean()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestTypeMappingClass>(nameof(TestTypeMappingClass.IsActive));

        // Act
        var result = _formatter.FormatMemberType(property, "boolean");

        // Assert
        Assert.Equal("boolean", result);
    }

    [Fact]
    public void FormatMemberType_WithCustomType_ReturnsTypeName()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestTypeMappingClass>(nameof(TestTypeMappingClass.Name));

        // Act
        var result = _formatter.FormatMemberType(property, "IUser");

        // Assert
        Assert.Equal("IUser", result);
    }

    [Fact]
    public void FormatMemberType_WithArray_ReturnsArrayType()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestTypeMappingClass>(nameof(TestTypeMappingClass.Name));

        // Act
        var result = _formatter.FormatMemberType(property, "string[]");

        // Assert
        Assert.Equal("string[]", result);
    }

    #endregion

    #region Guid Variations Tests

    [Theory]
    [InlineData("Guid")]
    [InlineData("guid")]
    [InlineData("GUID")]
    [InlineData("System.Guid")]
    [InlineData("IGuid")]
    public void FormatMemberType_WithGuidVariations_ReturnsString(string typeName)
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestTypeMappingClass>(nameof(TestTypeMappingClass.Id));

        // Act
        var result = _formatter.FormatMemberType(property, typeName);

        // Assert
        Assert.Equal("string", result);
    }

    #endregion

    #region Helper Methods

    private static TsProperty CreateTsPropertyFromType<T>(string propertyName)
    {
        var propertyInfo = typeof(T).GetProperty(propertyName)!;
        return new TsProperty(propertyInfo);
    }

    #endregion

    #region Test Types

    private class TestTypeMappingClass
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }

    #endregion
}

/// <summary>
/// Tests for nullable type detection.
/// </summary>
public class TypeFormatterNullabilityTests
{
    private readonly TypeFormatter _formatter = new(NullLogger.Instance);

    [Fact]
    public void FormatMemberType_WithPropertyInfo_DetectsNullableValueType()
    {
        // Arrange
        var propertyInfo = typeof(NullableTestClass).GetProperty(nameof(NullableTestClass.NullableInt))!;
        var property = new TsProperty(propertyInfo);

        // Act
        var result = _formatter.FormatMemberType(property, "number");

        // Assert
        Assert.Equal("number | null", result);
    }

    [Fact]
    public void FormatMemberType_WithPropertyInfo_NonNullableValueType_NoNull()
    {
        // Arrange
        var propertyInfo = typeof(NullableTestClass).GetProperty(nameof(NullableTestClass.NonNullableInt))!;
        var property = new TsProperty(propertyInfo);

        // Act
        var result = _formatter.FormatMemberType(property, "number");

        // Assert
        Assert.Equal("number", result);
    }

    [Fact]
    public void FormatMemberType_WithPropertyInfo_NullableGuid_ReturnsStringWithNull()
    {
        // Arrange
        var propertyInfo = typeof(NullableTestClass).GetProperty(nameof(NullableTestClass.NullableGuid))!;
        var property = new TsProperty(propertyInfo);

        // Act
        var result = _formatter.FormatMemberType(property, "Guid");

        // Assert
        Assert.Equal("string | null", result);
    }

    [Fact]
    public void FormatMemberType_WithPropertyInfo_NullableDateTime_ReturnsDateWithNull()
    {
        // Arrange
        var propertyInfo = typeof(NullableTestClass).GetProperty(nameof(NullableTestClass.NullableDateTime))!;
        var property = new TsProperty(propertyInfo);

        // Act
        var result = _formatter.FormatMemberType(property, "System.DateTime");

        // Assert
        Assert.Equal("Date | null", result);
    }

    private class NullableTestClass
    {
        public int? NullableInt { get; set; }
        public int NonNullableInt { get; set; }
        public Guid? NullableGuid { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public string? NullableString { get; set; }
        public string NonNullableString { get; set; } = string.Empty;
    }
}
