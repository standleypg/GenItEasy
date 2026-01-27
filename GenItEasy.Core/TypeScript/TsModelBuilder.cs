using System.Reflection;
using GenItEasy.TypeScript.Models;

namespace GenItEasy.TypeScript;

/// <summary>
/// Builds a TypeScript model from C# types.
/// </summary>
public class TsModelBuilder
{
    private readonly HashSet<Type> _types = [];

    /// <summary>
    /// Adds a type to the model.
    /// </summary>
    public TsModelBuilder Add(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        _types.Add(type);
        return this;
    }

    /// <summary>
    /// Builds the TypeScript model.
    /// </summary>
    public TsModel Build()
    {
        var model = new TsModel();
        var modulesByNamespace = new Dictionary<string, TsModule>();

        foreach (var type in _types)
        {
            var namespaceName = type.Namespace ?? "Global";

            if (!modulesByNamespace.TryGetValue(namespaceName, out var module))
            {
                module = new TsModule(namespaceName);
                modulesByNamespace[namespaceName] = module;
                model.Modules.Add(module);
            }

            if (type.IsEnum)
            {
                var tsEnum = BuildEnum(type);
                module.Enums.Add(tsEnum);
            }
            else
            {
                var tsClass = BuildClass(type);
                module.Classes.Add(tsClass);
            }
        }

        return model;
    }

    private static TsClass BuildClass(Type type)
    {
        var tsClass = new TsClass(type);

        // Get public instance properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            // Skip indexers
            if (prop.GetIndexParameters().Length > 0)
            {
                continue;
            }

            tsClass.Properties.Add(new TsProperty(prop));
        }

        // Get public instance fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            tsClass.Properties.Add(new TsProperty(field));
        }

        return tsClass;
    }

    private static TsEnum BuildEnum(Type type)
    {
        var tsEnum = new TsEnum(type);

        var names = Enum.GetNames(type);
        var values = Enum.GetValues(type);

        for (int i = 0; i < names.Length; i++)
        {
            var value = Convert.ChangeType(values.GetValue(i), Enum.GetUnderlyingType(type))!;
            tsEnum.Values.Add(new TsEnumValue(names[i], value));
        }

        return tsEnum;
    }
}
