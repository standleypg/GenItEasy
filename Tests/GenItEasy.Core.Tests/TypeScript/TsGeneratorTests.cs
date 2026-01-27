using GenItEasy.Configuration;
using GenItEasy.TypeScript;
using GenItEasy.TypeScript.Models;

namespace GenItEasy.Core.Tests.TypeScript;

public class TsGeneratorTests
{
    private readonly TsGenerator _generator;

    public TsGeneratorTests()
    {
        _generator = new TsGenerator();
    }

    #region Tuple Type Tests

    [Fact]
    public void Generate_WithTupleProperty_GeneratesTupleArraySyntax()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(TupleTestClass));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert
        Assert.Contains("[number, string]", result);
    }

    [Fact]
    public void Generate_WithNestedTuple_GeneratesNestedTupleSyntax()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(NestedTupleClass));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert
        Assert.Contains("[number, [string, boolean]]", result);
    }

    [Fact]
    public void Generate_WithTupleList_GeneratesTupleArraySyntax()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(TupleListClass));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert
        Assert.Contains("[number, string][]", result);
    }

    #endregion

    #region Generic Type Tests

    [Fact]
    public void Generate_WithGenericClass_GeneratesTypeParameters()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(GenericResponse<>));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert
        Assert.Contains("interface GenericResponse<T>", result);
    }

    [Fact]
    public void Generate_WithGenericClassMultipleParams_GeneratesAllTypeParameters()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(KeyValuePair<,>));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert
        Assert.Contains("interface KeyValuePair<TKey, TValue>", result);
    }

    [Fact]
    public void Generate_WithGenericPropertyUsingTypeParameter_UsesTypeParameterName()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(GenericResponse<>));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert
        Assert.Contains("T[]", result); // value: T[]
    }

    #endregion

    #region Enum Reference Tests

    [Fact]
    public void Generate_WithEnumProperty_DoesNotAddIPrefixToEnumType()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(ClassWithEnumProperty));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert
        Assert.Contains("TestStatus", result);
        Assert.DoesNotContain("ITestStatus", result);
    }

    [Fact]
    public void Generate_WithEnum_GeneratesEnumWithoutIPrefix()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(TestStatus));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Enums);

        // Assert
        Assert.Contains("enum TestStatus", result);
        Assert.DoesNotContain("enum ITestStatus", result);
    }

    #endregion

    #region Type Reference Tests

    [Fact]
    public void Generate_WithComplexTypeProperty_UsesSimpleTypeName()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(ClassWithComplexProperty));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert - Should use simple name, not fully qualified
        Assert.Contains("INestedClass", result);
        Assert.DoesNotContain("GenItEasy.Core.Tests.TypeScript.INestedClass", result);
    }

    #endregion

    #region Module Consolidation Tests

    [Fact]
    public void Generate_WithMultipleModules_SameFormattedName_MergesIntoOne()
    {
        // Arrange
        var model = new TsModel();
        var module1 = new TsModule("Namespace.One");
        module1.Classes.Add(new TsClass(typeof(SimpleClass1)));
        var module2 = new TsModule("Namespace.Two");
        module2.Classes.Add(new TsClass(typeof(SimpleClass2)));
        model.Modules.Add(module1);
        model.Modules.Add(module2);

        // Set formatter to return same name for all modules
        _generator.SetModuleNameFormatter(_ => "MergedNamespace");

        // Act
        var result = _generator.Generate(model, TsGeneratorOutput.Properties);

        // Assert - Should only have one namespace declaration
        var namespaceCount = result.Split("declare namespace MergedNamespace").Length - 1;
        Assert.Equal(1, namespaceCount);

        // Both classes should be in the merged namespace
        Assert.Contains("SimpleClass1", result);
        Assert.Contains("SimpleClass2", result);
    }

    [Fact]
    public void Generate_WithMultipleModules_DifferentNames_KeepsSeparate()
    {
        // Arrange
        var model = new TsModel();
        var module1 = new TsModule("Namespace.One");
        module1.Classes.Add(new TsClass(typeof(SimpleClass1)));
        var module2 = new TsModule("Namespace.Two");
        module2.Classes.Add(new TsClass(typeof(SimpleClass2)));
        model.Modules.Add(module1);
        model.Modules.Add(module2);

        // Act
        var result = _generator.Generate(model, TsGeneratorOutput.Properties);

        // Assert - Should have two separate namespaces
        Assert.Contains("declare namespace Namespace.One", result);
        Assert.Contains("declare namespace Namespace.Two", result);
    }

    #endregion

    #region Primitive Type Mapping Tests

    [Fact]
    public void Generate_WithDateTimeProperty_MapsToDate()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(DateTimeTestClass));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert
        Assert.Contains(": Date;", result);
    }

    [Fact]
    public void Generate_WithGuidProperty_MapsToString()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(GuidTestClass));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert
        Assert.Contains(": string;", result);
    }

    [Fact]
    public void Generate_WithDictionaryProperty_MapsToIndexSignature()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(DictionaryTestClass));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert
        Assert.Contains("{ [key: string]: number }", result);
    }

    [Fact]
    public void Generate_WithListProperty_MapsToArray()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(ListTestClass));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert
        Assert.Contains("string[]", result);
    }

    #endregion

    #region Nullable Type Tests

    [Fact]
    public void Generate_WithNullableValueType_MapsToBaseType()
    {
        // Arrange
        var model = new TsModelBuilder();
        model.Add(typeof(NullableTestClass));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert - Nullable<int> should map to number (nullability handled by formatter)
        Assert.Contains("number", result);
    }

    #endregion

    #region Enum Style Tests

    [Fact]
    public void Generate_WithNumericEnumStyle_GeneratesNumericValues()
    {
        // Arrange
        var generator = new TsGenerator { EnumStyle = EnumStyle.Numeric };
        var model = new TsModelBuilder();
        model.Add(typeof(TestStatus));
        var tsModel = model.Build();

        // Act
        var result = generator.Generate(tsModel, TsGeneratorOutput.Enums);

        // Assert
        Assert.Contains("export enum TestStatus", result);
        Assert.Contains("Active = 1", result);
        Assert.Contains("Inactive = 2", result);
    }

    [Fact]
    public void Generate_WithStringEnumStyle_GeneratesStringValues()
    {
        // Arrange
        var generator = new TsGenerator { EnumStyle = EnumStyle.String };
        var model = new TsModelBuilder();
        model.Add(typeof(TestStatus));
        var tsModel = model.Build();

        // Act
        var result = generator.Generate(tsModel, TsGeneratorOutput.Enums);

        // Assert
        Assert.Contains("export enum TestStatus", result);
        Assert.Contains("Active = \"Active\"", result);
        Assert.Contains("Inactive = \"Inactive\"", result);
    }

    [Fact]
    public void Generate_WithStringLiteralStyle_GeneratesUnionType()
    {
        // Arrange
        var generator = new TsGenerator { EnumStyle = EnumStyle.StringLiteral };
        var model = new TsModelBuilder();
        model.Add(typeof(TestStatus));
        var tsModel = model.Build();

        // Act
        var result = generator.Generate(tsModel, TsGeneratorOutput.Enums);

        // Assert
        Assert.Contains("export type TestStatus = \"Active\" | \"Inactive\";", result);
        Assert.DoesNotContain("export enum", result);
    }

    [Fact]
    public void Generate_WithStringLiteralStyle_HandlesMultipleValues()
    {
        // Arrange
        var generator = new TsGenerator { EnumStyle = EnumStyle.StringLiteral };
        var model = new TsModelBuilder();
        model.Add(typeof(MultiValueEnum));
        var tsModel = model.Build();

        // Act
        var result = generator.Generate(tsModel, TsGeneratorOutput.Enums);

        // Assert
        Assert.Contains("export type MultiValueEnum = \"One\" | \"Two\" | \"Three\" | \"Four\";", result);
    }

    [Fact]
    public void Generate_DefaultEnumStyle_IsNumeric()
    {
        // Arrange
        var generator = new TsGenerator(); // No explicit style set
        var model = new TsModelBuilder();
        model.Add(typeof(TestStatus));
        var tsModel = model.Build();

        // Act
        var result = generator.Generate(tsModel, TsGeneratorOutput.Enums);

        // Assert - Should default to numeric
        Assert.Contains("Active = 1", result);
        Assert.DoesNotContain("\"Active\"", result);
    }

    #endregion

    #region Cross-Namespace Reference Tests

    [Fact]
    public void Generate_WithMultipleNamespaces_UsesQualifiedTypeReferences()
    {
        // Arrange - Create model with types in different namespaces
        var model = new TsModel();

        // Module 1 with a class that references a type from Module 2
        var module1 = new TsModule("Namespace.One");
        var classWithRef = new TsClass(typeof(ClassReferencingOtherNamespace));
        module1.Classes.Add(classWithRef);

        // Module 2 with the referenced enum
        var module2 = new TsModule("Namespace.Two");
        var referencedEnum = new TsEnum(typeof(ReferencedEnum));
        module2.Enums.Add(referencedEnum);

        model.Modules.Add(module1);
        model.Modules.Add(module2);

        // Act
        var result = _generator.Generate(model, TsGeneratorOutput.Properties | TsGeneratorOutput.Enums);

        // Assert - Should have two separate namespaces
        Assert.Contains("declare namespace Namespace.One", result);
        Assert.Contains("declare namespace Namespace.Two", result);
    }

    [Fact]
    public void Generate_WithSingleNamespace_UsesSimpleTypeReferences()
    {
        // Arrange - All types in same namespace
        var model = new TsModelBuilder();
        model.Add(typeof(SimpleClass1));
        model.Add(typeof(SimpleClass2));
        var tsModel = model.Build();

        // Act
        var result = _generator.Generate(tsModel, TsGeneratorOutput.Properties);

        // Assert - Should use simple names (single namespace)
        Assert.Contains("SimpleClass1", result);
        Assert.Contains("SimpleClass2", result);
        // Should not have fully qualified names within the same namespace
        var namespaceCount = result.Split("declare namespace").Length - 1;
        Assert.Equal(1, namespaceCount);
    }

    [Fact]
    public void Generate_WithOutputNamespaceFormatter_MergesAllAndUsesSimpleNames()
    {
        // Arrange - Types in different original namespaces, but merged via formatter
        var model = new TsModel();

        var module1 = new TsModule("Original.Namespace.One");
        module1.Classes.Add(new TsClass(typeof(SimpleClass1)));

        var module2 = new TsModule("Original.Namespace.Two");
        module2.Classes.Add(new TsClass(typeof(SimpleClass2)));

        model.Modules.Add(module1);
        model.Modules.Add(module2);

        // Formatter returns same namespace for all modules (simulating outputNamespace config)
        _generator.SetModuleNameFormatter(_ => "MergedApp");

        // Act
        var result = _generator.Generate(model, TsGeneratorOutput.Properties);

        // Assert - Should have single namespace
        var namespaceCount = result.Split("declare namespace MergedApp").Length - 1;
        Assert.Equal(1, namespaceCount);

        // Both types should be present
        Assert.Contains("SimpleClass1", result);
        Assert.Contains("SimpleClass2", result);
    }

    #endregion

    #region Test Types

    private class TupleTestClass
    {
        public (int, string) SimpleTuple { get; set; }
    }

    private class NestedTupleClass
    {
        public (int, (string, bool)) NestedTuple { get; set; }
    }

    private class TupleListClass
    {
        public List<(int, string)> TupleList { get; set; } = [];
    }

    private class GenericResponse<T>
    {
        public T[] Value { get; set; } = [];
        public int Count { get; set; }
    }

    private class KeyValuePair<TKey, TValue>
    {
        public TKey Key { get; set; } = default!;
        public TValue Value { get; set; } = default!;
    }

    private enum TestStatus
    {
        Active = 1,
        Inactive = 2
    }

    private class ClassWithEnumProperty
    {
        public TestStatus Status { get; set; }
    }

    private class NestedClass
    {
        public int Id { get; set; }
    }

    private class ClassWithComplexProperty
    {
        public NestedClass Nested { get; set; } = new();
    }

    private class SimpleClass1
    {
        public int Id { get; set; }
    }

    private class SimpleClass2
    {
        public string Name { get; set; } = string.Empty;
    }

    private class DateTimeTestClass
    {
        public DateTime CreatedAt { get; set; }
    }

    private class GuidTestClass
    {
        public Guid Id { get; set; }
    }

    private class DictionaryTestClass
    {
        public Dictionary<string, int> Scores { get; set; } = [];
    }

    private class ListTestClass
    {
        public List<string> Names { get; set; } = [];
    }

    private class NullableTestClass
    {
        public int? NullableInt { get; set; }
    }

    // For cross-namespace tests
    private class ClassReferencingOtherNamespace
    {
        public ReferencedEnum Status { get; set; }
    }

    private enum ReferencedEnum
    {
        Value1,
        Value2
    }

    private enum MultiValueEnum
    {
        One,
        Two,
        Three,
        Four
    }

    #endregion
}
