using GenItEasy.Formatters;

namespace GenItEasy.Core.Tests.Formatters;

public class TypeVisibilityFormatterTests
{
    [Theory]
    [InlineData("Guid", false)]
    [InlineData("guid", false)]
    [InlineData("GUID", false)]
    [InlineData("System.Guid", false)]
    [InlineData("IGuid", false)]
    [InlineData("GuidValue", false)]
    [InlineData("MyGuidType", false)]
    public void IsTypeVisible_WithGuidTypes_ReturnsFalse(string typeName, bool expected)
    {
        // Act
        var result = TypeVisibilityFormatter.IsTypeVisible(typeName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("String", true)]
    [InlineData("Int32", true)]
    [InlineData("DateTime", true)]
    [InlineData("User", true)]
    [InlineData("Product", true)]
    [InlineData("OrderItem", true)]
    [InlineData("MyCustomClass", true)]
    public void IsTypeVisible_WithNonGuidTypes_ReturnsTrue(string typeName, bool expected)
    {
        // Act
        var result = TypeVisibilityFormatter.IsTypeVisible(typeName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsTypeVisible_WithEmptyString_ReturnsTrue()
    {
        // Act
        var result = TypeVisibilityFormatter.IsTypeVisible(string.Empty);

        // Assert
        Assert.True(result);
    }
}
