using GenItEasy.Formatters;
using TypeLitePlus;
using TypeLitePlus.TsModels;

namespace GenItEasy.Core.Tests.Formatters;

public class IdentifierFormatterTests
{
    #region FormatPropertyName Tests

    [Fact]
    public void FormatPropertyName_ConvertsFirstCharToLowercase()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestClass>(nameof(TestClass.UserName));

        // Act
        var result = IdentifierFormatter.FormatPropertyName(property);

        // Assert
        Assert.Equal("userName", result);
    }

    [Fact]
    public void FormatPropertyName_WithSingleCharacter_ConvertsToLowercase()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestClass>(nameof(TestClass.X));

        // Act
        var result = IdentifierFormatter.FormatPropertyName(property);

        // Assert
        Assert.Equal("x", result);
    }

    [Fact]
    public void FormatPropertyName_PreservesRemainingCase()
    {
        // Arrange
        var property = CreateTsPropertyFromType<TestClass>(nameof(TestClass.XMLParser));

        // Act
        var result = IdentifierFormatter.FormatPropertyName(property);

        // Assert
        Assert.Equal("xMLParser", result);
    }

    #endregion

    #region FormatModuleName Tests

    [Fact]
    public void FormatModuleName_AddsIPrefixToClasses()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add<SimpleUser>();
        var tsModel = model.Build();

        // Find the module and class
        var module = tsModel.Modules.First();
        var tsClass = module.Classes.First();
        var originalName = tsClass.Name;

        // Act
        IdentifierFormatter.FormatModuleName(module);

        // Assert
        Assert.StartsWith("I", tsClass.Name);
        Assert.Equal("I" + originalName, tsClass.Name);
    }

    [Fact]
    public void FormatModuleName_DoesNotAddPrefixToTypesStartingWithI()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add<ITestInterface>();
        var tsModel = model.Build();

        var module = tsModel.Modules.First();
        var tsClass = module.Classes.First();

        // Act
        IdentifierFormatter.FormatModuleName(module);

        // Assert - Should remain unchanged (already starts with I)
        Assert.StartsWith("I", tsClass.Name);
        Assert.DoesNotContain("II", tsClass.Name);
    }

    [Fact]
    public void FormatModuleName_DoesNotAddPrefixToEnums()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add<TestEnum>();
        var tsModel = model.Build();

        var module = tsModel.Modules.First();
        var tsEnum = module.Enums.First();
        var originalName = tsEnum.Name;

        // Act
        IdentifierFormatter.FormatModuleName(module);

        // Assert - Enums should not get I prefix
        Assert.Equal(originalName, tsEnum.Name);
        Assert.DoesNotContain("ITestEnum", tsEnum.Name);
    }

    [Fact]
    public void FormatModuleName_ReturnsModuleName()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add<SimpleUser>();
        var tsModel = model.Build();

        var module = tsModel.Modules.First();
        var expectedModuleName = module.Name;

        // Act
        var result = IdentifierFormatter.FormatModuleName(module);

        // Assert
        Assert.Equal(expectedModuleName, result);
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

    private class TestClass
    {
        public string UserName { get; set; } = string.Empty;
        public string X { get; set; } = string.Empty;
        public string XMLParser { get; set; } = string.Empty;
    }

    private class SimpleUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private interface ITestInterface
    {
        int Id { get; set; }
    }

    private enum TestEnum
    {
        Value1,
        Value2
    }

    #endregion
}
