using GenItEasy.TypeScript.Models;

namespace GenItEasy.Formatters;

/// <summary>
/// Handles identifier naming conventions (properties, interfaces, etc.).
/// </summary>
public abstract class IdentifierFormatter
{
    /// <summary>
    /// Converts property names to camelCase.
    /// </summary>
    public static string FormatPropertyName(TsProperty formatter)
    {
        if (string.IsNullOrEmpty(formatter.Name))
        {
            return formatter.Name;
        }

        return char.ToLowerInvariant(formatter.Name[0]) + formatter.Name[1..];
    }

    /// <summary>
    /// Formats module names and adds 'I' prefix to interfaces.
    /// </summary>
    /// <param name="formatter">The module to format.</param>
    /// <param name="outputNamespace">Optional custom output namespace. If provided, all modules will use this name.</param>
    public static string FormatModuleName(TsModule formatter, string? outputNamespace = null)
    {
        foreach (var member in formatter.Members)
        {
            // Skip if already has I prefix (I followed by uppercase letter, e.g., IUser, ITest)
            if (HasInterfacePrefix(member.Name))
            {
                continue;
            }

            // Skip generic type parameters (T, TKey, TValue, etc.)
            if (IsGenericTypeParameter(member.Name))
            {
                continue;
            }

            // Only add I prefix to classes (interfaces), NOT to enums
            if (member is TsClass)
            {
                member.Name = "I" + member.Name;
            }
        }

        // Use custom output namespace if provided, otherwise use original module name
        return !string.IsNullOrEmpty(outputNamespace) ? outputNamespace : formatter.Name;
    }

    /// <summary>
    /// Checks if a name already has an interface prefix (I followed by uppercase letter).
    /// Examples: IUser, ITest → true; IntegralTest, Item → false
    /// </summary>
    private static bool HasInterfacePrefix(string name)
    {
        return name.Length >= 2 &&
               name[0] == 'I' &&
               char.IsUpper(name[1]);
    }

    /// <summary>
    /// Checks if a name is likely a generic type parameter (T, TKey, TValue, etc.)
    /// </summary>
    private static bool IsGenericTypeParameter(string name)
    {
        return name.Length <= 6 &&
               name.StartsWith('T') &&
               (name.Length == 1 || char.IsUpper(name[1]));
    }
}