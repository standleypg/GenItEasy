using System.Collections;
using System.Text;
using GenItEasy.Configuration;
using GenItEasy.TypeScript.Models;

namespace GenItEasy.TypeScript;

/// <summary>
/// Generates TypeScript code from a TsModel.
/// </summary>
public class TsGenerator
{
    private Func<TsProperty, string>? _identifierFormatter;
    private Func<TsProperty, string, string>? _memberTypeFormatter;
    private Func<TsModule, string>? _moduleNameFormatter;
    private Func<object, string, bool>? _typeVisibilityFormatter;

    // Namespace mapping built during generation
    private Dictionary<string, string> _namespaceMapping = [];
    private bool _isSingleNamespaceMode;

    /// <summary>
    /// Gets or sets the enum generation style.
    /// </summary>
    public EnumStyle EnumStyle { get; init; } = EnumStyle.Numeric;

    /// <summary>
    /// Sets the formatter for property identifiers.
    /// </summary>
    public void SetIdentifierFormatter(Func<TsProperty, string> formatter)
    {
        _identifierFormatter = formatter;
    }

    /// <summary>
    /// Sets the formatter for member types.
    /// </summary>
    public void SetMemberTypeFormatter(Func<TsProperty, string, string> formatter)
    {
        _memberTypeFormatter = formatter;
    }

    /// <summary>
    /// Sets the formatter for module names.
    /// </summary>
    public void SetModuleNameFormatter(Func<TsModule, string> formatter)
    {
        _moduleNameFormatter = formatter;
    }

    /// <summary>
    /// Sets the formatter for type visibility.
    /// </summary>
    public void SetTypeVisibilityFormatter(Func<object, string, bool> formatter)
    {
        _typeVisibilityFormatter = formatter;
    }

    /// <summary>
    /// Generates TypeScript code from the model.
    /// </summary>
    public string Generate(TsModel model, TsGeneratorOutput output)
    {
        var sb = new StringBuilder();

        // Build namespace mapping and group modules by their formatted name
        var moduleGroups = new Dictionary<string, List<TsModule>>();
        _namespaceMapping = [];

        foreach (var module in model.Modules)
        {
            // Apply module name formatter (which may also modify class names)
            var moduleName = _moduleNameFormatter?.Invoke(module) ?? module.Name;

            // Store original -> formatted namespace mapping
            _namespaceMapping.TryAdd(module.Name, moduleName);

            if (!moduleGroups.TryGetValue(moduleName, out var group))
            {
                group = [];
                moduleGroups[moduleName] = group;
            }
            group.Add(module);
        }

        // Determine if all modules map to the same namespace
        _isSingleNamespaceMode = moduleGroups.Count == 1;

        // Generate each unique module, merging types from all source modules
        foreach (var (moduleName, modules) in moduleGroups)
        {
            sb.AppendLine($"declare namespace {moduleName} {{");

            if (output.HasFlag(TsGeneratorOutput.Properties))
            {
                foreach (var tsClass in modules.SelectMany(module => module.Classes))
                {
                    GenerateInterface(sb, tsClass);
                }
            }

            if (output.HasFlag(TsGeneratorOutput.Enums))
            {
                foreach (var tsEnum in modules.SelectMany(module => module.Enums))
                {
                    GenerateEnum(sb, tsEnum);
                }
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private void GenerateInterface(StringBuilder sb, TsClass tsClass)
    {
        // Check visibility
        if (_typeVisibilityFormatter != null && !_typeVisibilityFormatter(tsClass, tsClass.Name))
        {
            return;
        }

        // Build interface name with type parameters if generic
        var interfaceName = tsClass.Name;
        if (tsClass.TypeParameters.Count > 0)
        {
            interfaceName = $"{tsClass.Name}<{string.Join(", ", tsClass.TypeParameters)}>";
        }

        sb.AppendLine($"\texport interface {interfaceName} {{");

        foreach (var prop in tsClass.Properties)
        {
            var propName = _identifierFormatter?.Invoke(prop) ?? prop.Name;
            var typeName = GetTypeScriptType(prop);
            var formattedTypeName = _memberTypeFormatter?.Invoke(prop, typeName) ?? typeName;

            sb.AppendLine($"\t\t{propName}: {formattedTypeName};");
        }

        sb.AppendLine("\t}");
    }

    private void GenerateEnum(StringBuilder sb, TsEnum tsEnum)
    {
        // Check visibility
        if (_typeVisibilityFormatter != null && !_typeVisibilityFormatter(tsEnum, tsEnum.Name))
        {
            return;
        }

        switch (EnumStyle)
        {
            case EnumStyle.StringLiteral:
                GenerateStringLiteralType(sb, tsEnum);
                break;
            case EnumStyle.String:
                GenerateStringEnum(sb, tsEnum);
                break;
            case EnumStyle.Numeric:
            default:
                GenerateNumericEnum(sb, tsEnum);
                break;
        }
    }

    private void GenerateNumericEnum(StringBuilder sb, TsEnum tsEnum)
    {
        sb.AppendLine($"\texport enum {tsEnum.Name} {{");

        for (int i = 0; i < tsEnum.Values.Count; i++)
        {
            var value = tsEnum.Values[i];
            var comma = i < tsEnum.Values.Count - 1 ? "," : "";
            sb.AppendLine($"\t\t{value.Name} = {value.Value}{comma}");
        }

        sb.AppendLine("\t}");
    }

    private void GenerateStringEnum(StringBuilder sb, TsEnum tsEnum)
    {
        sb.AppendLine($"\texport enum {tsEnum.Name} {{");

        for (int i = 0; i < tsEnum.Values.Count; i++)
        {
            var value = tsEnum.Values[i];
            var comma = i < tsEnum.Values.Count - 1 ? "," : "";
            sb.AppendLine($"\t\t{value.Name} = \"{value.Name}\"{comma}");
        }

        sb.AppendLine("\t}");
    }

    private static void GenerateStringLiteralType(StringBuilder sb, TsEnum tsEnum)
    {
        var values = tsEnum.Values.Select(v => $"\"{v.Name}\"");
        sb.AppendLine($"\texport type {tsEnum.Name} = {string.Join(" | ", values)};");
    }

    /// <summary>
    /// Gets the TypeScript type for a property.
    /// </summary>
    private string GetTypeScriptType(TsProperty prop)
    {
        var type = prop.PropertyType;
        return MapType(type);
    }

    /// <summary>
    /// Maps a CLR type to a TypeScript type.
    /// </summary>
    private string MapType(Type type)
    {
        // Handle nullable value types
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            return MapType(underlyingType);
        }

        // Handle arrays
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            return $"{MapType(elementType)}[]";
        }

        // Handle generic collections
        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();

            // ValueTuple<T1, T2, ...> -> [T1, T2, ...]
            if (IsValueTupleType(genericDef))
            {
                var typeArgs = type.GetGenericArguments();
                var mappedArgs = typeArgs.Select(MapType);
                return $"[{string.Join(", ", mappedArgs)}]";
            }

            // List<T>, IList<T>, ICollection<T>, IEnumerable<T>
            if (genericDef == typeof(List<>) ||
                genericDef == typeof(IList<>) ||
                genericDef == typeof(ICollection<>) ||
                genericDef == typeof(IEnumerable<>))
            {
                var elementType = type.GetGenericArguments()[0];
                return $"{MapType(elementType)}[]";
            }

            // Dictionary<K, V>
            if (genericDef == typeof(Dictionary<,>) ||
                genericDef == typeof(IDictionary<,>))
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                return $"{{ [key: {MapType(keyType)}]: {MapType(valueType)} }}";
            }

            // Nullable<T> handled above
        }

        // Handle IEnumerable (non-generic) - typically object[]
        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            return "any[]";
        }

        // Primitive types
        return type.FullName switch
        {
            "System.String" => "string",
            "System.Char" => "string",
            "System.Boolean" => "boolean",
            "System.Byte" => "number",
            "System.SByte" => "number",
            "System.Int16" => "number",
            "System.UInt16" => "number",
            "System.Int32" => "number",
            "System.UInt32" => "number",
            "System.Int64" => "number",
            "System.UInt64" => "number",
            "System.Single" => "number",
            "System.Double" => "number",
            "System.Decimal" => "number",
            "System.DateTime" => "Date",
            "System.DateTimeOffset" => "Date",
            "System.DateOnly" => "Date",
            "System.TimeOnly" => "string",
            "System.TimeSpan" => "string",
            "System.Guid" => "string",
            "System.Object" => "any",
            _ => GetComplexTypeName(type)
        };
    }

    /// <summary>
    /// Checks if a type is a ValueTuple generic type.
    /// </summary>
    private static bool IsValueTupleType(Type type)
    {
        return type.FullName?.StartsWith("System.ValueTuple`", StringComparison.Ordinal) == true;
    }

    /// <summary>
    /// Gets the TypeScript name for a complex type.
    /// </summary>
    private string GetComplexTypeName(Type type)
    {
        // Handle generic type parameters (T, TKey, TValue, etc.)
        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        // For enums and classes
        if (type.IsEnum || type.IsClass || type.IsInterface || type is { IsValueType: true, IsPrimitive: false })
        {
            var name = type.Name;

            // Handle generic types
            if (type.IsGenericType)
            {
                var backtickIndex = name.IndexOf('`');
                if (backtickIndex > 0)
                {
                    name = name[..backtickIndex];
                }

                var typeArgs = type.GetGenericArguments();
                var mappedArgs = typeArgs.Select(MapType);
                name = $"{name}<{string.Join(", ", mappedArgs)}>";
            }

            // Enums don't get I prefix, classes/interfaces do
            var typeName = type.IsEnum ? name : $"I{name}";

            // In single namespace mode, use simple names
            // In multi-namespace mode, use qualified names
            if (_isSingleNamespaceMode || string.IsNullOrEmpty(type.Namespace))
            {
                return typeName;
            }

            // Get the formatted namespace for this type
            var formattedNamespace = _namespaceMapping.TryGetValue(type.Namespace, out var ns)
                ? ns
                : type.Namespace;

            return $"{formattedNamespace}.{typeName}";
        }

        return "any";
    }
}
