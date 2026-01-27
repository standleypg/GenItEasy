using GenItEasy.TypeScript;
using GenItEasy.TypeScript.Models;

namespace GenItEasy.Core.Tests.TypeScript;

public class TsModelBuilderTests
{
    #region Add Type Tests

    [Fact]
    public void Add_WithClass_AddsToModel()
    {
        // Arrange
        var builder = new TsModelBuilder();

        // Act
        builder.Add(typeof(SimpleClass));
        var model = builder.Build();

        // Assert
        Assert.Single(model.Modules);
        Assert.Single(model.Modules[0].Classes);
        Assert.Equal("SimpleClass", model.Modules[0].Classes[0].Name);
    }

    [Fact]
    public void Add_WithEnum_AddsToEnums()
    {
        // Arrange
        var builder = new TsModelBuilder();

        // Act
        builder.Add(typeof(TestEnum));
        var model = builder.Build();

        // Assert
        Assert.Single(model.Modules);
        Assert.Single(model.Modules[0].Enums);
        Assert.Equal("TestEnum", model.Modules[0].Enums[0].Name);
    }

    [Fact]
    public void Add_WithGenericArgument_AddsToModel()
    {
        // Arrange
        var builder = new TsModelBuilder();

        // Act
        builder.Add(typeof(GenericClass<string>));
        var model = builder.Build();

        // Assert
        Assert.Single(model.Modules);
        Assert.Single(model.Modules[0].Classes);
    }

    #endregion

    #region Generic Type Parameter Tests

    [Fact]
    public void Build_WithGenericTypeDefinition_ExtractsTypeParameters()
    {
        // Arrange
        var builder = new TsModelBuilder();
        builder.Add(typeof(GenericClass<>));

        // Act
        var model = builder.Build();

        // Assert
        var tsClass = model.Modules[0].Classes[0];
        Assert.Single(tsClass.TypeParameters);
        Assert.Equal("T", tsClass.TypeParameters[0]);
    }

    [Fact]
    public void Build_WithMultipleTypeParameters_ExtractsAllParameters()
    {
        // Arrange
        var builder = new TsModelBuilder();
        builder.Add(typeof(MultiGenericClass<,>));

        // Act
        var model = builder.Build();

        // Assert
        var tsClass = model.Modules[0].Classes[0];
        Assert.Equal(2, tsClass.TypeParameters.Count);
        Assert.Equal("TKey", tsClass.TypeParameters[0]);
        Assert.Equal("TValue", tsClass.TypeParameters[1]);
    }

    [Fact]
    public void Build_WithNonGenericClass_HasNoTypeParameters()
    {
        // Arrange
        var builder = new TsModelBuilder();
        builder.Add(typeof(SimpleClass));

        // Act
        var model = builder.Build();

        // Assert
        var tsClass = model.Modules[0].Classes[0];
        Assert.Empty(tsClass.TypeParameters);
    }

    #endregion

    #region Namespace Grouping Tests

    [Fact]
    public void Build_WithSameNamespace_GroupsIntoOneModule()
    {
        // Arrange
        var builder = new TsModelBuilder();
        builder.Add(typeof(SimpleClass));
        builder.Add(typeof(AnotherClass));

        // Act
        var model = builder.Build();

        // Assert
        Assert.Single(model.Modules);
        Assert.Equal(2, model.Modules[0].Classes.Count);
    }

    [Fact]
    public void Build_WithDifferentNamespaces_CreatesMultipleModules()
    {
        // Arrange
        var builder = new TsModelBuilder();
        builder.Add(typeof(SimpleClass)); // This namespace
        builder.Add(typeof(string)); // System namespace

        // Act
        var model = builder.Build();

        // Assert
        Assert.Equal(2, model.Modules.Count);
    }

    #endregion

    #region Property Extraction Tests

    [Fact]
    public void Build_ExtractsPublicProperties()
    {
        // Arrange
        var builder = new TsModelBuilder();
        builder.Add(typeof(ClassWithProperties));

        // Act
        var model = builder.Build();

        // Assert
        var tsClass = model.Modules[0].Classes[0];
        Assert.Equal(2, tsClass.Properties.Count);
        Assert.Contains(tsClass.Properties, p => p.Name == "PublicProp1");
        Assert.Contains(tsClass.Properties, p => p.Name == "PublicProp2");
    }

    [Fact]
    public void Build_IgnoresPrivateProperties()
    {
        // Arrange
        var builder = new TsModelBuilder();
        builder.Add(typeof(ClassWithPrivateProperty));

        // Act
        var model = builder.Build();

        // Assert
        var tsClass = model.Modules[0].Classes[0];
        Assert.Single(tsClass.Properties);
        Assert.Equal("PublicProp", tsClass.Properties[0].Name);
    }

    [Fact]
    public void Build_IgnoresStaticProperties()
    {
        // Arrange
        var builder = new TsModelBuilder();
        builder.Add(typeof(ClassWithStaticProperty));

        // Act
        var model = builder.Build();

        // Assert
        var tsClass = model.Modules[0].Classes[0];
        Assert.Single(tsClass.Properties);
        Assert.Equal("InstanceProp", tsClass.Properties[0].Name);
    }

    [Fact]
    public void Build_IgnoresIndexers()
    {
        // Arrange
        var builder = new TsModelBuilder();
        builder.Add(typeof(ClassWithIndexer));

        // Act
        var model = builder.Build();

        // Assert
        var tsClass = model.Modules[0].Classes[0];
        Assert.Single(tsClass.Properties);
        Assert.Equal("Name", tsClass.Properties[0].Name);
    }

    #endregion

    #region Enum Value Extraction Tests

    [Fact]
    public void Build_ExtractsEnumValues()
    {
        // Arrange
        var builder = new TsModelBuilder();
        builder.Add(typeof(TestEnum));

        // Act
        var model = builder.Build();

        // Assert
        var tsEnum = model.Modules[0].Enums[0];
        Assert.Equal(3, tsEnum.Values.Count);
        Assert.Contains(tsEnum.Values, v => v.Name == "Value1" && (int)v.Value == 0);
        Assert.Contains(tsEnum.Values, v => v.Name == "Value2" && (int)v.Value == 1);
        Assert.Contains(tsEnum.Values, v => v.Name == "Value3" && (int)v.Value == 2);
    }

    [Fact]
    public void Build_ExtractsEnumWithExplicitValues()
    {
        // Arrange
        var builder = new TsModelBuilder();
        builder.Add(typeof(ExplicitValueEnum));

        // Act
        var model = builder.Build();

        // Assert
        var tsEnum = model.Modules[0].Enums[0];
        Assert.Contains(tsEnum.Values, v => v.Name == "Active" && (int)v.Value == 10);
        Assert.Contains(tsEnum.Values, v => v.Name == "Inactive" && (int)v.Value == 20);
    }

    #endregion

    #region Test Types

    private class SimpleClass
    {
        public int Id { get; set; }
    }

    private class AnotherClass
    {
        public string Name { get; set; } = string.Empty;
    }

    private class GenericClass<T>
    {
        public T Value { get; set; } = default!;
    }

    private class MultiGenericClass<TKey, TValue>
    {
        public TKey Key { get; set; } = default!;
        public TValue Value { get; set; } = default!;
    }

    private class ClassWithProperties
    {
        public int PublicProp1 { get; set; }
        public string PublicProp2 { get; set; } = string.Empty;
    }

    private class ClassWithPrivateProperty
    {
        public int PublicProp { get; set; }
        private string PrivateProp { get; set; } = string.Empty;
    }

    private class ClassWithStaticProperty
    {
        public int InstanceProp { get; set; }
        public static string StaticProp { get; set; } = string.Empty;
    }

    private class ClassWithIndexer
    {
        public string Name { get; set; } = string.Empty;
        public int this[int index] => index;
    }

    private enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }

    private enum ExplicitValueEnum
    {
        Active = 10,
        Inactive = 20
    }

    #endregion
}
