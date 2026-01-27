using GenItEasy.Formatters;
using GenItEasy.TypeScript;
using GenItEasy.TypeScript.Models;

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
        model.Add(typeof(SimpleUser));
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
        model.Add(typeof(ITestInterface));
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
        model.Add(typeof(TestEnum));
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
        model.Add(typeof(SimpleUser));
        var tsModel = model.Build();

        var module = tsModel.Modules.First();
        var expectedModuleName = module.Name;

        // Act
        var result = IdentifierFormatter.FormatModuleName(module);

        // Assert
        Assert.Equal(expectedModuleName, result);
    }

    [Fact]
    public void FormatModuleName_AddsIPrefixToTypesStartingWithIFollowedByLowercase()
    {
        // Arrange - IntegralTest starts with 'I' but 'n' is lowercase, so it needs 'I' prefix
        var model = new TsModelBuilder();
        model.Add(typeof(IntegralTest));
        var tsModel = model.Build();

        var module = tsModel.Modules.First();
        var tsClass = module.Classes.First();

        // Act
        IdentifierFormatter.FormatModuleName(module);

        // Assert - Should add I prefix because 'I' is not followed by uppercase
        Assert.Equal("IIntegralTest", tsClass.Name);
    }

    [Fact]
    public void FormatModuleName_DoesNotAddPrefixToTypesWithInterfaceNamingConvention()
    {
        // Arrange - IUserService starts with 'I' followed by uppercase 'U'
        var model = new TsModelBuilder();
        model.Add(typeof(IUserService));
        var tsModel = model.Build();

        var module = tsModel.Modules.First();
        var tsClass = module.Classes.First();

        // Act
        IdentifierFormatter.FormatModuleName(module);

        // Assert - Should NOT add another I prefix
        Assert.Equal("IUserService", tsClass.Name);
        Assert.DoesNotContain("IIUserService", tsClass.Name);
    }

    [Fact]
    public void FormatModuleName_WithOutputNamespace_ReturnsCustomNamespace()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(SimpleUser));
        var tsModel = model.Build();

        var module = tsModel.Modules.First();

        // Act
        var result = IdentifierFormatter.FormatModuleName(module, "CustomNamespace");

        // Assert
        Assert.Equal("CustomNamespace", result);
    }

    [Fact]
    public void FormatModuleName_WithNullOutputNamespace_ReturnsOriginalModuleName()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(SimpleUser));
        var tsModel = model.Build();

        var module = tsModel.Modules.First();
        var expectedModuleName = module.Name;

        // Act
        var result = IdentifierFormatter.FormatModuleName(module, null);

        // Assert
        Assert.Equal(expectedModuleName, result);
    }

    [Fact]
    public void FormatModuleName_WithEmptyOutputNamespace_ReturnsOriginalModuleName()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(SimpleUser));
        var tsModel = model.Build();

        var module = tsModel.Modules.First();
        var expectedModuleName = module.Name;

        // Act
        var result = IdentifierFormatter.FormatModuleName(module, "");

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

    // Type that starts with 'I' but is NOT an interface-prefixed name
    private class IntegralTest
    {
        public int Value { get; set; }
    }

    // Type that follows interface naming convention (I + uppercase)
    private interface IUserService
    {
        int Id { get; set; }
    }

    #endregion
}
